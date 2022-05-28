
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Entities.eShopDapr;
using Common.Http;

namespace Client.UseCases.eShopDapr.Workers
{

    public class DataIngestor
    {

        private readonly HttpRequest httpRequest;

        // no setup if failers (just redo whole process or extend the thread time)
        public DataIngestor(HttpRequest httpRequest)
        {
            this.httpRequest = httpRequest;
        }

        public void RunCatalog(string url, List<CatalogItem> items)
        {
            Console.WriteLine($"Data are generated. Next step is to insert them in DB, inserting {items.Count} items");
            Console.WriteLine("press any key to contine the process...");
            Console.ReadKey();
            int n = items.Count;
            Task[] taskArray = new Task[n];
            Console.WriteLine("Item");
            for (int i = 0; i < taskArray.Length; i++)
            {
                HttpContent payload = new StringContent(JsonSerializer.Serialize(items[i]), System.Text.Encoding.UTF8, "application/json");
                Console.WriteLine(JsonSerializer.Serialize(items[i]));
                Task<string> result = httpRequest.GetResponseStatus(url, payload);
                taskArray[i] = result;
            }
            Console.WriteLine();
            try
            {
                Task.WaitAll(taskArray);
                int i = 1;
                foreach (Task<string> result in taskArray)
                {
                    Console.WriteLine($"Task {i} returned {result.Result}");
                    i++;
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                Console.WriteLine("Catalog items inserted. Check. Continue to basket inserting.");
                Console.WriteLine("press any key to contine the process...");
                Console.ReadKey();
            }
        }

        public void RunBasket(string url, List<string> basketIds, List<CatalogItem> catalogItems, int MIN_NUM_ITEMS, int MAX_NUM_ITEMS)
        {
            // will pick the items from the catalog
            // and create a basket which the user can use as his selection
            int n = basketIds.Count;
            Task[] taskArray = new Task[n];
            Console.WriteLine("Basket");
            for (int i = 0; i < taskArray.Length; i++)
            {
                CustomerBasket basket = new CustomerBasket();
                basket.BuyerId = basketIds[i];
                basket.Items = DataGenerator.GenerateBasketForExistingItems(new Random().Next(MIN_NUM_ITEMS, MAX_NUM_ITEMS), catalogItems);
                Console.WriteLine(JsonSerializer.Serialize(basket));
                HttpContent payload = new StringContent(JsonSerializer.Serialize(basket), System.Text.Encoding.UTF8, "application/json");
                Task<string> result = httpRequest.GetResponseStatus(url, payload);
                taskArray[i] = result;
            }
            try
            {
                Task.WaitAll(taskArray);
                int i = 1;
                foreach (Task<string> result in taskArray)
                {
                    Console.WriteLine($"Task {i} returned {result.Result}");
                    i++;
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                Console.WriteLine("Basket items inserted. Check. Continue to transactions.");
                Console.WriteLine("press any key to contine the process...");
                Console.ReadKey();
            }
        }
    }
}
