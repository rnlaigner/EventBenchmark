using System;
using System.Collections.Generic;
using Client.Configuration;
using Client.UseCases.eShopDapr.Transactions;

namespace Client.UseCases.eShopDapr
{
    public class EShopDaprDefaultConfig : IUseCaseConfig
    {

        public EShopDaprDefaultConfig() { }

        public List<string> GetTransactions()
        {
            //return new List<string> { typeof(Checkout).Name, typeof(DeleteProduct).Name, typeof(PriceUpdate).Name, typeof(StockReplenishment).Name };
            throw new NotImplementedException();
        }

        public List<string> GetDistributionOfTransactions(List<int> transactionDistribution)
        {
            if (transactionDistribution.Count != 4)
            {
                throw new Exception("Transaction distribution needs to have 4 values");
            }
            int checkout = transactionDistribution[0];
            int delete = transactionDistribution[1];
            int update = transactionDistribution[2];
            int replenish = transactionDistribution[3];

            int total = checkout + delete + update + replenish;
            Console.WriteLine("Transactions:");
            Console.WriteLine($"Checkout in {((double)checkout) / total} %, {checkout} of {total}");
            Console.WriteLine($"Delete in {((double)delete) / total} %, {delete} of {total}");
            Console.WriteLine($"Update in {((double)update) / total} %, {update} of {total}");
            Console.WriteLine($"Replenish in {((double)replenish) / total} %, {replenish} of {total}");
            Console.WriteLine("press any key to contine the process...");
            Console.ReadKey();
            List<string> list = new List<string>();
            for (int i = 0; i < delete; i++)
                list.Add(typeof(DeleteProduct).Name);
            for (int i = 0; i < update; i++)
                list.Add(typeof(PriceUpdate).Name);
            for (int i = 0; i < replenish; i++)
                list.Add(typeof(StockReplenishment).Name);
            for (int i = 0; i < checkout; i++)
                list.Add(typeof(Checkout).Name);
            return list;

        }
        public List<int> GetPercentageOfTransactions()
        {
            //return new List<int> { 100, 0, 0, 0 };
            throw new NotImplementedException();
        }

        public TimeSpan? TimeLimit()
        {
            //return null;
            throw new NotImplementedException();
        }

        public List<int> GetNumberOfRequestsPerTransaction()
        {
            //return new List<int> { 0, 0, 0, 0 };
            throw new NotImplementedException();
        }

        public List<TimeSpan> GetPeriodBetweenRequestsOfSameTransaction()
        {
            //return new List<TimeSpan> { new TimeSpan(2000) };
            throw new NotImplementedException();
        }

        public Distribution GetDistribution()
        {
            //return Distribution.NORMAL;
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetUrlMap() {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Basket
            // POST
            dictionary.Add("basket", "http://"+ Constants.BASKET_IP + "/api/v1/Basket");
            // POST
            dictionary.Add("checkout", "http://" + Constants.BASKET_IP + "/api/v1/Basket/checkout");
            
            // Catalog
            // POST
            dictionary.Add("catalog", "http://" + Constants.CATALOG_IP + "/api/v1/Catalog/items");
            // DELETE
            dictionary.Add("delete", "http://" + Constants.CATALOG_IP + "/api/v1/Catalog/");
            // PUT
            dictionary.Add("update", "http://" + Constants.CATALOG_IP + "/api/v1/Catalog/items");
            // PUT
            dictionary.Add("replenishment", "http://" + Constants.CATALOG_IP + "/api/v1/Catalog/addStocks");


            // Order
            // see if any URL needed

            // Payment
            // NO PAYMENT URL 

            return dictionary;
        }
    }
}
