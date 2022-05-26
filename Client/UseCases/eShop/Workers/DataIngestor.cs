
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
            for (int i = 0; i < taskArray.Length; i++)
            {
                HttpContent payload = new StringContent(JsonSerializer.Serialize(items[i]), System.Text.Encoding.UTF8, "application/json");
                taskArray[i] = Task.Run(() => httpClient.PostAsync(new Uri(url), payload));
            }
            Task.WaitAll(taskArray);
        }

        public void RunBasket(string url, List<string> basketIds, List<CatalogItem> catalogItems)
        {
            // will pick the items from the catalog
            // and create a basket which the user can use as his selection
            int n = basketIds.Count;
            Task[] taskArray = new Task[n];
            for (int i = 0; i < taskArray.Length; i++)
            {
                CustomerBasket basket = new CustomerBasket();
                basket.BuyerId = basketIds[i];
                basket.Items = DataGenerator.GenerateBasketForExistingItems(5, catalogItems);
                Console.WriteLine("Basket");
                Console.WriteLine(JsonSerializer.Serialize(basket));
                Console.WriteLine("--------");
                HttpContent payload = new StringContent(JsonSerializer.Serialize(basket), System.Text.Encoding.UTF8, "application/json");
                taskArray[i] = Task.Run(() => httpClient.PostAsync(new Uri(url), payload));
            }
            Task.WaitAll(taskArray);
        }
    }
}
