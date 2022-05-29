using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Client.Infra;
using Client.UseCases.eShopDapr.TransactionInput;
using Client.UseCases.eShopDapr.Transactions;
using Client.UseCases.eShopDapr.Workers;
using Common.Entities.eShopDapr;
using Common.Http;

/**
 * 
     1 - select the scenario of the execution

     2 - each scenario has specific atributes
     
     3 - initial the run of scenario

     4 - insert items, customers, baskets

     5 - run transactions based on scenario distribution

     6 - run timer/manually
 * 
 */
namespace Client.UseCases.eShopDapr
{
    public class EShopDaprUseCase : Stoppable
    {

        private readonly IUseCaseConfig Config;
        private readonly string Scenario;

        public EShopDaprUseCase(IUseCaseConfig Config, string Scenario) : base()
        {
            this.Config = Config;
            this.Scenario = Scenario;
        }
        public void Init()
        {
            Console.WriteLine($"Running scenario: {Scenario}");
            switch (Scenario)
            {

                case Constants.REFERENTIAL_INTEGRITY:
                    {
                        var items = 100;
                        var customers = 100;
                        var minItems = 1;
                        var maxItems = 10;
                        var minQuantityItems = 1;
                        var maxQuantityItems = 10;
                        Console.WriteLine($"items: {items}");
                        Console.WriteLine($"customers: {customers}");
                        Console.WriteLine($"minItems: {minItems}");
                        Console.WriteLine($"maxItems: {maxItems}");
                        Console.WriteLine($"minQuantityItems: {minQuantityItems}");
                        Console.WriteLine($"maxQuantityItems: {maxQuantityItems}");
                        // transaction checkout, delete, update, replenish
                        List<int> transactions = new List<int>() { 10, 1, 1, 0 };
                        runScenario(items, customers, minItems, maxItems, minQuantityItems, maxQuantityItems, transactions, false);
                        break;
                    }
                case Constants.NON_NEGATIVE_ENFORCEMENT:
                    {
                        // many customers, with items that have low stock so they will get removed
                        var items = 10;
                        var customers = 100;
                        var minItems = 10;
                        var maxItems = 10;
                        var minQuantityItems = 1;
                        var maxQuantityItems = 1;
                        Console.WriteLine($"items: {items}");
                        Console.WriteLine($"customers: {customers}");
                        Console.WriteLine($"minItems: {minItems}");
                        Console.WriteLine($"maxItems: {maxItems}");
                        Console.WriteLine($"minQuantityItems: {minQuantityItems}");
                        Console.WriteLine($"maxQuantityItems: {maxQuantityItems}");
                        // transaction checkout, delete, update, replenish
                        List<int> transactions = new List<int>() { 100, 0, 0, 0 };
                        runScenario(items, customers, minItems, maxItems, minQuantityItems, maxQuantityItems, transactions, false);
                        break;
                    }
                case Constants.EXACTLY_ONCE_PROCESSING:
                    {
                        // here need a specific case:
                        //    - a lot of same customer requests
                        //    - a normal execution where we see if the payment had multiple same processe
                        var items = 100;
                        var customers = 50;
                        var minItems = 1;
                        var maxItems = 10;
                        var minQuantityItems = 50;
                        var maxQuantityItems = 200;
                        Console.WriteLine($"items: {items}");
                        Console.WriteLine($"customers: {customers}");
                        Console.WriteLine($"minItems: {minItems}");
                        Console.WriteLine($"maxItems: {maxItems}");
                        Console.WriteLine($"minQuantityItems: {minQuantityItems}");
                        Console.WriteLine($"maxQuantityItems: {maxQuantityItems}");
                        // transaction checkout, delete, update, replenish
                        List<int> transactions = new List<int>() { 100, 0, 1, 1 };
                        runScenario(items, customers, minItems, maxItems, minQuantityItems, maxQuantityItems, transactions, false);
                        break;
                    }
                case Constants.PERFORMANCE:
                    {
                        // workload need to be differnt
                        var items = 400;
                        var customers = 50;
                        var minItems = 1;
                        var maxItems = 15;
                        var minQuantityItems = 1000;
                        var maxQuantityItems = 10000;
                        Console.WriteLine($"items: {items}");
                        Console.WriteLine($"customers: {customers}");
                        Console.WriteLine($"minItems: {minItems}");
                        Console.WriteLine($"maxItems: {maxItems}");
                        Console.WriteLine($"minQuantityItems: {minQuantityItems}");
                        Console.WriteLine($"maxQuantityItems: {maxQuantityItems}");
                        // transaction checkout, delete, update, replenish
                        List<int> transactions = new List<int>() { 15, 0, 1, 4 };
                        runScenario(items, customers, minItems, maxItems, minQuantityItems, maxQuantityItems, transactions, false);
                        break;
                    }
                case Constants.SIMPLE_SUCESS_FLOW:
                    {
                        var items = 100;
                        var customers = 1;
                        var minItems = 1;
                        var maxItems = 10;
                        var minQuantityItems = 1;
                        var maxQuantityItems = 10;
                        Console.WriteLine($"items: {items}");
                        Console.WriteLine($"customers: {customers}");
                        Console.WriteLine($"minItems: {minItems}");
                        Console.WriteLine($"maxItems: {maxItems}");
                        Console.WriteLine($"minQuantityItems: {minQuantityItems}");
                        Console.WriteLine($"maxQuantityItems: {maxQuantityItems}");
                        // transaction checkout, delete, update, replenish
                        List<int> transactions = new List<int>() { 1, 0, 0, 0 };
                        runScenario(items, customers, minItems, maxItems, minQuantityItems, maxQuantityItems, transactions, true);
                        break;
                    }
                default:
                    Console.WriteLine("NO SCENARIO CHOSEN");
                    break;
            }
        }

        private void runScenario(int NUM_TOTAL_ITEMS, int NUM_CUSTOMERS, int MIN_NUM_ITEMS, int MAX_NUM_ITEMS, int MIN_ITEM_QTY, int MAX_ITEM_QTY, List<int> TRANSACTIONS, bool runOnce)
        {
            List<ApplicationUser> customers = DataGenerator.GenerateCustomers(NUM_CUSTOMERS);
            List<CatalogItem> items = DataGenerator.GenerateCatalogItems(NUM_TOTAL_ITEMS, MIN_ITEM_QTY, MAX_ITEM_QTY);
            List<string> customeBaskets = DataGenerator.GenerateGuids(NUM_CUSTOMERS);

            CheckoutTransactionInput checkoutTransactionInput = new CheckoutTransactionInput
            {
                MinNumItems = MIN_NUM_ITEMS,
                MaxNumItems = MAX_NUM_ITEMS,
                MinItemQty = MIN_ITEM_QTY,
                MaxItemQty = MAX_ITEM_QTY,
                CartUrl = Config.GetUrlMap()["checkout"],
                Users = customers,
                BasketIds = customeBaskets
            };

            DeleteProductTransactionInput deleteProductTransactionInput = new DeleteProductTransactionInput
            {
                NumTotalItems = NUM_TOTAL_ITEMS,
                CatalogUrl = Config.GetUrlMap()["delete"],
                Items = items
            };

            PriceUpdateTransactionInput priceUpdateTransactionInput = new PriceUpdateTransactionInput
            {
                NumTotalItems = NUM_TOTAL_ITEMS,
                CatalogUrl = Config.GetUrlMap()["update"],
                Items = items
            };

            StockReplenishmentTransactionInput stockReplenishmentTransactionInput = new StockReplenishmentTransactionInput
            {
                NumTotalItems = NUM_TOTAL_ITEMS,
                CatalogUrl = Config.GetUrlMap()["replenishment"],
                Items = items
            };

            HttpRequest httpRequest = new HttpRequest();

            DataIngestor dataIngestor = new DataIngestor(httpRequest);

            // blocking call
            dataIngestor.RunCatalog(Config.GetUrlMap()["catalog"], items);
            dataIngestor.RunBasket(Config.GetUrlMap()["basket"], customeBaskets, items, MIN_NUM_ITEMS, MAX_NUM_ITEMS);

            // TODO setup event listeners before submitting the transactions
            // TODO create payment provider, a grain that will randomly accept or deny payments

            // now that data is stored, we can start the transacions
            Console.WriteLine("Starting the transaction. Verify that the baskets and items are in place.");
            Console.WriteLine("press any key to contine the process...");
            Console.ReadKey();

            Dictionary<string, IInput> inputs = new Dictionary<string, IInput>();
            inputs.Add(typeof(Checkout).Name, checkoutTransactionInput);
            inputs.Add(typeof(DeleteProduct).Name, deleteProductTransactionInput);
            inputs.Add(typeof(PriceUpdate).Name, priceUpdateTransactionInput);
            inputs.Add(typeof(StockReplenishment).Name, stockReplenishmentTransactionInput);

            RunTransactions(inputs, TRANSACTIONS, runOnce);
        }

        private void RunTransactions(Dictionary<string, IInput> inputs, List<int> transactionDistribution, bool runOnce)
        {

            Random random = new Random();

            var transactions = Config.GetDistributionOfTransactions(transactionDistribution);
            int n = transactions.Count;

            // build and run all transaction tasks
            int iterations = 0; // temporal during testing
            // keep added items to avoid repetition
            HashSet<int> basketsCheckout = new HashSet<int>();

            while (!IsStopped())
            {
                iterations++;
                int k = random.Next(0, n - 1);
                Console.WriteLine($"Round: {iterations}");
                switch (transactions[k])
                {

                    case "Checkout":
                        {
                            // define number of items in the cart
                            CheckoutTransactionInput input = (CheckoutTransactionInput)inputs[typeof(Checkout).Name];
                            long numberItems = random.Next(input.MinNumItems, input.MaxNumItems);

                            HttpClient client = new HttpClient();
                            Checkout checkout = new Checkout(client, input);
                            var userId = new Random().Next(input.Users.Count);
                            if (!basketsCheckout.Contains(userId))
                            {
                                basketsCheckout.Add(userId);
                            }
                            else {
                                Console.WriteLine($"Duplicate request, {userId}");
                                //break;
                            }
                            var payload = new BasketCheckout()
                            {
                                City = input.Users[userId].City,
                                Street = input.Users[userId].Street,
                                State = input.Users[userId].State,
                                Country = input.Users[userId].Country,
                                CardNumber = input.Users[userId].CardNumber,
                                CardHolderName = input.Users[userId].CardHolderName,
                                CardExpiration = input.Users[userId].CardExpiration,
                                CardSecurityCode = input.Users[userId].SecurityNumber,
                                UserEmail = input.Users[userId].Name,
                                UserId = input.BasketIds[userId]
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
                            try
                            {
                                t.Wait();
                            }
                            catch (AggregateException ae)
                            {
                                foreach (var ex in ae.InnerExceptions)
                                {
                                    Console.WriteLine("ERROR");
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            // add to concurrent queue and check if error is too many requests, if so, send again later
                            //   this is to maintain the distribution
                            // tracking by error message - no retries

                            break;
                        }

                    case "DeleteProduct":
                        {
                            HttpClient client = new HttpClient();
                            DeleteProductTransactionInput input = (DeleteProductTransactionInput)inputs[typeof(DeleteProduct).Name];

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
                // used for the all other
                if (iterations % 100 == 0)
                {
                    var time = 1000;
                    Console.WriteLine($"Processing the requests, sleep for {time} ms");
                    Thread.Sleep(time);
                }

                // used for the last worflow
                /*if (iterations % 50 == 0)
                {
                    // give time for the system to process all
                    var time = 3000;
                    Console.WriteLine($"Processing the requests, sleep for {time} ms");
                    Thread.Sleep(time);
                }*/
                // for simple case to see the succesful work
                if (runOnce)
                {
                    break;
                }
            }

        }
        // NOT USED
        /*private NumberGenerator GetDistribution(int NUM_TOTAL_ITEMS)
        {
            if( Config.GetDistribution() == Distribution.NORMAL)
            {
                return new UniformLongGenerator(1,NUM_TOTAL_ITEMS);
            }
            return new ZipfianGenerator(NUM_TOTAL_ITEMS);
        }*/
    }
}
