using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Client.Configuration;
using Client.Infra;
using Client.UseCases.eShopDapr.TransactionInput;
using Client.UseCases.eShopDapr.Transactions;
using Client.UseCases.eShopDapr.Workers;
using Common.Entities.eShopDapr;
using Common.YCSB;

/**
 * 
     1- define the use case OK

     2 - define the transactions and respective percentage   OK

     3 - define the distribution of each transaction    OK

     4 - setup data generation   OK

     5 - setup workers to submit requests   OK

     6 - setup event listeners (rabbit mq) given the config
 * 
 */
namespace Client.UseCases.eShopDapr
{
    public class EShopDaprUseCase : Stoppable
    {

        private readonly IUseCaseConfig Config;

        public EShopDaprUseCase(IUseCaseConfig Config) : base()
        {
            this.Config = Config;
        }

        // the constructor are expected to be the same..
        // but creating so many classes is not good, i believe i should model as tasks....
        // FIXME https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming
        // automatic thread pool control

        // orleans can used as reliable metadatada and configuration storage, as well as programming abstraction run...
        // some tasks are better off orleans?

        public void Init()
        {

            List<ApplicationUser> customers =  DataGenerator.GenerateCustomers(Constants.CUST_PER_DIST);
            List<CatalogItem> items = DataGenerator.GenerateCatalogItems(Constants.NUM_TOTAL_ITEMS);
            List<string> customeBaskets = DataGenerator.GenerateGuids(Constants.CUST_PER_DIST);

            CheckoutTransactionInput checkoutTransactionInput = new CheckoutTransactionInput
            {
                MinNumItems = Constants.MIN_NUM_ITEMS,
                MaxNumItems = Constants.MAX_NUM_ITEMS,
                MinItemQty = Constants.MIN_ITEM_QTY,
                MaxItemQty = Constants.MAX_ITEM_QTY,
                CartUrl = Config.GetUrlMap()["checkout"],
                Users = customers,
                BasketIds = customeBaskets
            };

            DeleteProductTransactionInput deleteProductTransactionInput = new DeleteProductTransactionInput
            {
                NumTotalItems = Constants.NUM_TOTAL_ITEMS,
                CatalogUrl = Config.GetUrlMap()["delete"],
                Items = items
            };

            PriceUpdateTransactionInput priceUpdateTransactionInput = new PriceUpdateTransactionInput
            {
                NumTotalItems = Constants.NUM_TOTAL_ITEMS,
                CatalogUrl = Config.GetUrlMap()["update"],
                Items = items
            };

            StockReplenishmentTransactionInput stockReplenishmentTransactionInput = new StockReplenishmentTransactionInput
            {
                NumTotalItems = Constants.NUM_TOTAL_ITEMS,
                CatalogUrl = Config.GetUrlMap()["replenishment"],
                Items = items
            };


            HttpClient httpClient = new HttpClient();

            DataIngestor dataIngestor = new DataIngestor(httpClient);

            // blocking call
            dataIngestor.RunCatalog(Config.GetUrlMap()["catalog"], items);
            dataIngestor.RunBasket(Config.GetUrlMap()["basket"], customeBaskets, items);

            // TODO setup event listeners before submitting the transactions


            // TODO create payment provider, a grain that will randomly accept or deny payments


            // now that data is stored, we can start the transacions
            Dictionary<string, IInput> inputs = new Dictionary<string, IInput>();
            inputs.Add(typeof(Checkout).Name, checkoutTransactionInput);
            inputs.Add(typeof(DeleteProduct).Name, deleteProductTransactionInput);
            inputs.Add(typeof(PriceUpdate).Name, priceUpdateTransactionInput);
            inputs.Add(typeof(StockReplenishment).Name, stockReplenishmentTransactionInput);
            RunTransactions(inputs);

        }

        /**
         * TODO possibly use the holistic task scheduler
         * 
         */
        private void RunTransactions(Dictionary<string, IInput> inputs)
        {

            Random random = new Random();
            //HttpClient client = new HttpClient();

            //int userCount = 0;

            int n = Config.GetTransactions().Count;

            // build and run all transaction tasks

            while (!IsStopped())
            {

                int k = random.Next(0, n-1);
                Console.WriteLine($"Random number k: {k}");
                switch (Config.GetTransactions()[k])
                {

                    case "Checkout":
                        {

                            NumberGenerator numberGenerator = GetDistribution();

                            // define number of items in the cart
                            CheckoutTransactionInput input = (CheckoutTransactionInput) inputs[typeof(Checkout).Name];
                            long numberItems = random.Next(input.MinNumItems, input.MaxNumItems);

                            // keep added items to avoid repetition
                            Dictionary<int, string> usedItemIds = new Dictionary<int, string>();
                            
                            HttpClient client = new HttpClient();
                            Checkout checkout = new Checkout(client, input);
                            var userId = new Random().Next(input.Users.Count);
                            var payload = new BasketCheckout()
                            {
                                City = input.Users[userId].City,
                                Street = input.Users[userId].Street,
                                State = input.Users[userId].State,
                                Country = input.Users[userId].Country,
                                ZipCode = input.Users[userId].ZipCode,
                                CardNumber = input.Users[userId].CardNumber,
                                CardHolderName = input.Users[userId].CardHolderName,
                                CardExpiration = input.Users[userId].CardExpiration,
                                CardSecurityNumber = input.Users[userId].SecurityNumber,
                                CardTypeId = input.Users[userId].CardType,
                                Buyer = input.Users[userId].Name,
                                UserId = input.BasketIds[userId],
                            };

                            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                            client.DefaultRequestHeaders.Add("x-requestid", input.BasketIds[userId]);

                            // now checkout
                            Console.WriteLine();
                            Console.WriteLine("CHECKOUT");
                            Console.WriteLine(JsonSerializer.Serialize(payload));
                            Console.WriteLine(client.DefaultRequestHeaders);
                            Console.WriteLine();
                            Task t = Task.Run(() => client.PostAsync(input.CartUrl, content));
                            
                            // add to concurrent queue and check if error is too many requests, if so, send again later
                            //   this is to maintain the distribution

                            break;
                        }

                    case "DeleteProduct":
                        {
                            HttpClient client = new HttpClient();
                            DeleteProductTransactionInput input = (DeleteProductTransactionInput)inputs[typeof(DeleteProduct).Name];
                            DeleteProduct deleteProduct = new DeleteProduct(GetDistribution(), input);

                            int id = input.Items[new Random().Next(input.Items.Count)].Id;

                            // now checkout
                            Console.WriteLine();
                            Console.WriteLine("DELETE");
                            Console.WriteLine(id);
                            Console.WriteLine();
                            Task t = Task.Run(() => client.DeleteAsync(input.CatalogUrl+id));

                            break;
                        }

                    case "PriceUpdate":
                        {
                            HttpClient client = new HttpClient();
                            PriceUpdateTransactionInput input = (PriceUpdateTransactionInput)inputs[typeof(PriceUpdate).Name];
                            PriceUpdate priceUpdate = new PriceUpdate(GetDistribution(), input);

                            CatalogItem payload = input.Items[new Random().Next(input.Items.Count)];
                            payload.Price = (decimal)(new Random().NextDouble() * 100);

                            // now update price
                            Console.WriteLine();
                            Console.WriteLine("PRICE");
                            Console.WriteLine(JsonSerializer.Serialize(payload));
                            Console.WriteLine();

                            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                            Task t = Task.Run(() => client.PutAsync(input.CatalogUrl, content));

                            break;
                        }

                    case "StockReplenishment":
                        {
                            HttpClient client = new HttpClient();
                            StockReplenishmentTransactionInput input = (StockReplenishmentTransactionInput)inputs[typeof(StockReplenishment).Name];
                            StockReplenishment stockReplenishment = new StockReplenishment();

                            CatalogItem item = input.Items[new Random().Next(input.Items.Count)];

                            AddStock payload = new AddStock
                            {
                                Id = item.Id,
                                StockToAdd = (int)(new Random().NextDouble() * 100)
                            };
                            

                            // now update stock
                            Console.WriteLine();
                            Console.WriteLine("STOCK");
                            Console.WriteLine(JsonSerializer.Serialize(payload));
                            Console.WriteLine();

                            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                            Task t = Task.Run(() => client.PutAsync(input.CatalogUrl, content));

                            break;
                        }

                }


            }

        }

        private NumberGenerator GetDistribution()
        {
            if( Config.GetDistribution() == Distribution.NORMAL)
            {
                return new UniformLongGenerator(1,Constants.NUM_TOTAL_ITEMS);
            }

            return new ZipfianGenerator(Constants.NUM_TOTAL_ITEMS);
        }

    }
}
