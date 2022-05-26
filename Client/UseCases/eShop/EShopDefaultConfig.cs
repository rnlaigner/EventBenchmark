using System;
using System.Collections.Generic;
using Client.Configuration;
using Client.UseCases.eShop.Transactions;

namespace Client.UseCases.eShop
{
    public class EShopDefaultConfig : IUseCaseConfig
    {

        private const string BASKET_IP = "localhost:5103";
        private const string CATALOG_IP = "localhost:5101";
        private const string ORDER_IP = "localhost:5102";


        public EShopDefaultConfig() { }

        public List<string> GetTransactions()
        {
            return new List<string> { typeof(Checkout).Name };
        }

        public List<string> GetDistributionOfTransactions() { 
            List<string> list = new List<string>();
            // list will have 10 elements and distribution will be as folowwing
            // 10 % delete
            // 10 % update
            // 80 % checkout
            list.Add(typeof(DeleteProduct).Name);
            list.Add(typeof(PriceUpdate).Name);
            for (int i = 0; i < 8; i++)
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

        List<string> IUseCaseConfig.GetTransactions()
        {
            return new List<string>() { typeof(Checkout).Name };
        }

        public Distribution GetDistribution()
        {
            return Distribution.NORMAL;
        }

        public Dictionary<string, string> GetUrlMap() {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            
            // Basket
            dictionary.Add("basket", "http://"+ BASKET_IP + "/api/v1/Basket");
            dictionary.Add("checkout", "http://" + BASKET_IP + "/api/v1/Basket/checkout");
            
            // Catalog
            dictionary.Add("catalog", "http://" + CATALOG_IP + "/catalog-api/api/v1/Catalog/items");
            /*dictionary.Add("deleteCatalog", "http://" + CATALOG_IP + "/api/v1/Basket/checkout");

            // Order
            dictionary.Add("updateCatalog", "http://" + ORDER_IP + "/api/v1/Basket/checkout");
            dictionary.Add("deleteCatalog", "http://" + ORDER_IP + "/api/v1/Basket/checkout");
            */

            // Payment
            // NO PAYMENT URL 

            return dictionary;
        }
    }
}
