﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Http;
using Common.Scenario.Customer;
using Common.Scenario.Entity;
using Common.Streaming;
using Common.YCSB;
using GrainInterfaces.Workers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Orleans.Streams;

namespace Grains.Workers
{

    /**
     * Nice example of tasks in a customer session:
     * https://github.com/GoogleCloudPlatform/microservices-demo/blob/main/src/loadgenerator/locustfile.py
     */
    public sealed class CustomerWorker : Grain, ICustomerWorker
    {
        private readonly Random random;

        private CustomerConfiguration config;

        private NumberGenerator sellerIdGenerator;

        private IStreamProvider streamProvider;

        private IAsyncStream<Event> stream;

        private IAsyncStream<CustomerStatusUpdate> txStream;

        private long customerId;

        private CustomerStatus status;

        private string productUrl;

        private string cartUrl;

        private Customer customer;

        private readonly ILogger<CustomerWorker> _logger;

        public override async Task OnActivateAsync()
        {
            this.customerId = this.GetPrimaryKeyLong();
            this.streamProvider = this.GetStreamProvider(StreamingConfiguration.DefaultStreamProvider);
            this.stream = streamProvider.GetStream<Event>(StreamingConfiguration.CustomerReactStreamId, this.customerId.ToString());
            var subscriptionHandles = await stream.GetAllSubscriptionHandles();
            if (subscriptionHandles.Count > 0)
            {
                foreach (var subscriptionHandle in subscriptionHandles)
                {
                    await subscriptionHandle.ResumeAsync(ProcessEventAsync);
                }
            }
            await stream.SubscribeAsync<Event>(ProcessEventAsync);

            var workloadStream = streamProvider.GetStream<int>(StreamingConfiguration.CustomerStreamId, this.customerId.ToString());
            var subscriptionHandles_ = await workloadStream.GetAllSubscriptionHandles();
            if (subscriptionHandles_.Count > 0)
            {
                foreach (var subscriptionHandle in subscriptionHandles_)
                {
                    await subscriptionHandle.ResumeAsync(Run);
                }
            }
            await workloadStream.SubscribeAsync<int>(Run);

            this.txStream = streamProvider.GetStream<CustomerStatusUpdate>(StreamingConfiguration.CustomerStreamId, StreamingConfiguration.TransactionStreamNameSpace);

        }

        public CustomerWorker(ILogger<CustomerWorker> logger)
        {
            this._logger = logger;
            this.random = new Random();
        }

        public async Task Init(CustomerConfiguration config)
        {
            this.config = config;
            this.sellerIdGenerator = this.config.sellerDistribution == Distribution.UNIFORM ?
                new UniformLongGenerator(this.config.sellerRange.Start.Value, this.config.sellerRange.End.Value) :
                new ZipfianGenerator(this.config.sellerRange.Start.Value, this.config.sellerRange.End.Value);
            this.productUrl = this.config.urls["products"];
            this.cartUrl = this.config.urls["carts"];
            this.customer = await GetCustomer(this.config.urls["customers"], customerId);
        }

        private static async Task<Customer> GetCustomer(string customerUrl, long customerId)
        {
            HttpResponseMessage resp = await Task.Run(async () =>
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, customerUrl + "/" + customerId);
                return await HttpUtils.client.SendAsync(message);
            });
            var str = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Customer>(str);
        }

        /**
         * From the list of browsed keys, picks randomly the keys to checkout
         */
        private ISet<long> DefineKeysToCheckout(List<long> browsedKeys, int numberOfKeysToCheckout)
        {
            ISet<long> set = new HashSet<long>(numberOfKeysToCheckout);
            while (set.Count < numberOfKeysToCheckout)
            {
                set.Add(browsedKeys[random.Next(0, browsedKeys.Count)]);
            }
            return set;
        }

        private async Task<ISet<long>> DefineKeysToBrowseAsync(int numberOfKeysToBrowse)
        {
            ISet<long> keyMap = new HashSet<long>(numberOfKeysToBrowse);
            ISellerWorker sellerWorker;
            StringBuilder sb = new StringBuilder();
            long grainId;
            long productId;
            for (int i = 0; i < numberOfKeysToBrowse; i++)
            {
                grainId = this.sellerIdGenerator.NextValue();
                sellerWorker = GrainFactory.GetGrain<ISellerWorker>(grainId);

                // we dont measure the performance of the benchmark, only the system. as long as we can submit enough workload we are fine
                productId = await sellerWorker.GetProductId();
                while (keyMap.Contains(productId))
                {
                    grainId = this.sellerIdGenerator.NextValue();
                    sellerWorker = GrainFactory.GetGrain<ISellerWorker>(grainId);
                    productId = await sellerWorker.GetProductId();
                }

                keyMap.Add(productId);
                sb.Append(productId);
                if (i < numberOfKeysToBrowse - 1) sb.Append(", ");
            }
            _logger.LogInformation("Customer {0} defined the keys to browse: {1}", this.customerId, sb.ToString());
            return keyMap;
        }

        private async Task Run(int obj, StreamSequenceToken token)
        {
            _logger.LogInformation("Customer {0} started!", this.customerId);

            if (this.config.delayBeforeStart > 0)
            {
                _logger.LogInformation("Customer {0} delay before start: {1}", this.customerId, this.config.delayBeforeStart);
                await Task.Delay(this.config.delayBeforeStart);
            }
            else
            {
                _logger.LogInformation("Customer {0} NO delay before start!", this.customerId);
            }

            int numberOfKeysToBrowse = random.Next(1, this.config.maxNumberKeysToBrowse + 1);

            _logger.LogInformation("Customer {0} has this number of keys to browse: {1}", customerId, numberOfKeysToBrowse);

            var keyMap = await DefineKeysToBrowseAsync(numberOfKeysToBrowse);

            _logger.LogInformation("Customer {0} will start browsing", this.customerId);

            // browsing
            Browse(keyMap);

            _logger.LogInformation("Customer " + customerId + " finished browsing!");

            // TODO should we also model this behavior?
            int numberOfKeysToCheckout =
                random.Next(1, Math.Min(numberOfKeysToBrowse, this.config.maxNumberKeysToAddToCart) + 1);

            // adding to cart
            AddToCart(DefineKeysToCheckout(keyMap.ToList(), numberOfKeysToCheckout));

            // get cart
            GetCart();

            // checkout
            Checkout();

            return;
        }

        // diretiva que vai ouvir do orleans
        // method

        /**
         * Simulating the customer browsing the cart before checkout
         */
        private async void GetCart()
        {
            await Task.Run(() =>
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, cartUrl + "/" + customerId);

                // faria um push to stream do orleans streams

                return HttpUtils.client.Send(message);
            });
        }

        private async void Checkout()
        {
            // define whether client should send a checkout request
            if (this.config.checkoutDistribution[random.Next(0, this.config.checkoutDistribution.Length)] == 0)
            {
                this.status = CustomerStatus.CHECKOUT_NOT_SENT;
                _logger.LogInformation("Customer " + customerId + " decided not to send a checkout!");
                return;
            }

            _logger.LogInformation("Customer " + customerId + " decided to send a checkout!");

            // inform checkout intent. optional feature

            HttpResponseMessage resp = await Task.Run(async () =>
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, cartUrl + "/" + customerId + "/checkout");
                message.Content = BuildCheckoutPayload(this.customerId, this.customer);
                return await HttpUtils.client.SendAsync(message);
            });

            if (resp != null && resp.IsSuccessStatusCode)
            {
                this.status = CustomerStatus.CHECKOUT_SENT;
                await txStream.OnNextAsync(new CustomerStatusUpdate(this.customerId, this.status));
                _logger.LogInformation("Customer " + customerId + " sent the checkout sucessfully");
            }
            else
            {
                this.status = CustomerStatus.CHECKOUT_FAILED;
                _logger.LogInformation("Customer " + customerId + " checkout failed");
            }

        }

        private async void AddToCart(ISet<long> keyMap)
        {
            HttpResponseMessage response;
            foreach (var productId in keyMap)
            {
                _logger.LogInformation("Customer {0} adding product {1} to cart", this.customerId, productId);
                int delay = this.random.Next(this.config.delayBetweenRequestsRange.Start.Value, this.config.delayBetweenRequestsRange.End.Value + 1);
                await Task.Run(async () =>
                {
                    try
                    {
                        response = await HttpUtils.client.GetAsync(productUrl + "/" + productId);

                        // add to cart
                        if (response.Content.Headers.ContentLength == 0)
                        {
                            _logger.LogInformation("Response content for product {0} is empty! {1}", productId);
                            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        }

                        var qty = random.Next(this.config.minMaxQtyRange.Start.Value, this.config.minMaxQtyRange.End.Value);

                        var productRet = await response.Content.ReadAsStringAsync();
                        var payload = BuildCartItem(productRet, qty);

                        response = await HttpUtils.client.PostAsync(cartUrl + "/" + customerId + "/add", payload);

                        return response;
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation("Exception Message: {0} Customer {1} Url {2} Key {3}", e.Message, customerId, productUrl, productId);
                        if (e is HttpRequestException)
                        {
                            HttpRequestException e_ = (HttpRequestException)e;
                            return new HttpResponseMessage((HttpStatusCode)e_.StatusCode);
                        }
                        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    }
                    finally
                    {
                        // artificial delay after adding the product
                        await Task.Delay(delay);
                    }

                });
            }
        }

        private async void Browse(ISet<long> keyMap)
        {
            int delay;
            foreach (var productId in keyMap)
            {
                _logger.LogInformation("Customer {0} browsing product {1}", this.customerId, productId);
                delay = this.random.Next(this.config.delayBetweenRequestsRange.Start.Value, this.config.delayBetweenRequestsRange.End.Value + 1);
                await Task.Run(async () =>
                {
                    try
                    {
                        await HttpUtils.client.GetAsync(productUrl + "/" + productId); 
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation("Exception Message: {0} Customer {1} Url {2} Key {3}", e.Message, customerId, productUrl, productId);
                    }

                });
                // artificial delay after retrieving the product
                await Task.Delay(delay);
            }
        }

        private async Task ProcessEventAsync(Event data, StreamSequenceToken token)
        {
            // for each event, forward to the respective handler
            switch (data.topic)
            {
                case "abandoned-cart": { await this.ReactToAbandonedCart(data.payload); break; }
                case "payment-rejected": { await this.ReactToPaymentRejected(data.payload); break; }
                case "out-of-stock": { await this.ReactToOutOfStock(data.payload); break; }
                case "price-update": { await this.ReactToPriceUpdate(data.payload); break; }
                case "product-unavailable": { await this.ReactToProductUnavailable(data.payload); break; }
                default: { _logger.LogInformation("Topic: " + data.topic + " has no associated reaction in customer grain " + customerId); break; }
            }
            return;
        }

        // AFTER CHECKOUT

        private Task ReactToAbandonedCart(string abandonedCartEvent)
        {
            // TODO given a probability, can checkout or not
            return Task.CompletedTask;
        }

        private Task ReactToPaymentRejected(string paymentDeniedEvent)
        {
            // random. insert a new payment type or cancel the order
            return Task.CompletedTask;
        }

        private Task ReactToOutOfStock(string outOfStockEvent)
        {
            return Task.CompletedTask;
        }

        // ON CHECKOUT

        private Task ReactToPriceUpdate(string priceUpdateEvent)
        {
            // submit some operations.. get request (customer url) and then a post (rating)
            return Task.CompletedTask;
        }

        private Task ReactToProductUnavailable(string priceUpdateEvent)
        {
            // 
            return Task.CompletedTask;
        }

        private static StringContent BuildCartItem(string productPayload, int quantity)
        {
            Product product = JsonConvert.DeserializeObject<Product>(productPayload);
            // build a basket item
            BasketItem basketItem = new()
            {
                ProductId = product.id,
                SellerId = product.seller_id,
                // ProductName = product.name,
                UnitPrice = product.price,
                Quantity = quantity,
                FreightValue = 0 // not modeling freight value in this version
            };
            var payload = JsonConvert.SerializeObject(basketItem);
            return HttpUtils.BuildPayload(payload);
        }

        private static StringContent BuildCheckoutPayload(long customerId, Customer customer)
        {

            // TODO define payment type from distribution
            // TODO define voucher from distribution

            // build
            CustomerCheckout basketCheckout = new()
            {
                CustomerId = customerId,
                City = customer.city,
                Street = customer.address,
                State = customer.state,
                ZipCode = customer.zip_code_prefix,
                CardNumber = customer.card_number,
                CardHolderName = customer.card_holder_name,
                CardExpiration = customer.card_expiration,
                CardSecurityNumber = customer.card_security_number,
                CardBrand = customer.card_type
            };
            var payload = JsonConvert.SerializeObject(basketCheckout);
            return HttpUtils.BuildPayload(payload);
        }

    }
}