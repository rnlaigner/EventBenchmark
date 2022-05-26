using System;
using System.Collections.Generic;
using Client.Configuration;
using Client.UseCases.eShopDapr.Transactions;

namespace Client.UseCases.eShopDapr
{
    public class EShopDaprDefaultConfig : IUseCaseConfig
    {

        private const string BASKET_IP = "localhost:5103";
        private const string CATALOG_IP = "localhost:5101";
        private const string ORDER_IP = "localhost:5102";


        public EShopDaprDefaultConfig() { }

        public List<string> GetTransactions()
        {
            return new List<string> { typeof(Checkout).Name, typeof(DeleteProduct).Name, typeof(PriceUpdate).Name, typeof(StockReplenishment).Name };
        }

        public List<string> GetDistributionOfTransactions()
        {
            List<string> list = new List<string>();
            // list will have 10 elements and distribution will be as folowwing
            // 10 % delete
            // 10 % update
            // 10 % stock replanishment
            // 70 % checkout
            list.Add(typeof(DeleteProduct).Name);
            list.Add(typeof(PriceUpdate).Name);
            list.Add(typeof(StockReplenishment).Name);
            for (int i = 0; i < 7; i++)
                list.Add(typeof(Checkout).Name);
            return list;

        }

        public List<int> GetPercentageOfTransactions()
        {
            return new List<int> { 100, 0, 0 };
        }

        public TimeSpan? TimeLimit()
        {
            return null;
        }

        public List<int> GetNumberOfRequestsPerTransaction()
        {
            return new List<int> { 0, 0, 0 };
        }

        public List<TimeSpan> GetPeriodBetweenRequestsOfSameTransaction()
        {
            return new List<TimeSpan> { new TimeSpan(2000) };
        }

        public Distribution GetDistribution()
        {
            return Distribution.NORMAL;
        }

        public Dictionary<string, string> GetUrlMap() {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Basket
            // POST
            dictionary.Add("basket", "http://"+ BASKET_IP + "/api/v1/Basket");
            // POST
            dictionary.Add("checkout", "http://" + BASKET_IP + "/api/v1/Basket/checkout");
            
            // Catalog
            // POST
            dictionary.Add("catalog", "http://" + CATALOG_IP + "/api/v1/Catalog/items");
            // DELETE
            dictionary.Add("delete", "http://" + CATALOG_IP + "/api/v1/Catalog/");
            // PUT
            dictionary.Add("update", "http://" + CATALOG_IP + "/api/v1/Catalog/items");
            // PUT
            dictionary.Add("replenishment", "http://" + CATALOG_IP + "/api/v1/Catalog/addStocks");


            // Order
            // see if any URL needed

            // Payment
            // NO PAYMENT URL 

            return dictionary;
        }
    }
}
