
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Entities.eShop;

namespace Client.UseCases.eShop.Workers
{

    public class DataIngestor
    {

        private readonly HttpClient httpClient;

        private readonly Queue<KeyValuePair<string, HttpContent>> PendingRequests;

        private int _retries;

        public DataIngestor(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.PendingRequests = new();
            this._retries = 1;
        }

        public void RunCatalog(string url, List<CatalogItem> items) {
            int n = items.Count;
            Task[] taskArray = new Task[n];
            Console.WriteLine("Item");
            for (int i = 0; i < taskArray.Length; i++)
            {
                HttpContent payload = new StringContent(JsonSerializer.Serialize(items[i]), System.Text.Encoding.UTF8, "application/json");
                Console.WriteLine(JsonSerializer.Serialize(items[i]));
                taskArray[i] = Task.Run(() => httpClient.PostAsync(new Uri(url), payload));
            }
            Console.WriteLine();
            try
            {
                Task.WaitAll(taskArray);
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void RunBasket(string url, List<string> basketIds, List<CatalogItem> catalogItems)
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
                basket.Items = DataGenerator.GenerateBasketForExistingItems(new Random().Next(Constants.MIN_NUM_ITEMS, Constants.MAX_NUM_ITEMS), catalogItems);
                Console.WriteLine(JsonSerializer.Serialize(basket));
                HttpContent payload = new StringContent(JsonSerializer.Serialize(basket), System.Text.Encoding.UTF8, "application/json");
                taskArray[i] = Task.Run(() => httpClient.PostAsync(new Uri(url), payload));
            }
            Console.WriteLine("--------");
            try
            {
                Task.WaitAll(taskArray);
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
