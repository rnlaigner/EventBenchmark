using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities.eShop;

namespace Client.UseCases.eShop
{
    public static class DataGenerator
    {

        const string numbers = "0123456789";
        const string alphanumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static List<CatalogItem> GenerateCatalogItems(int NumberOfItems)
        {
            List<CatalogItem> items = new List<CatalogItem>(NumberOfItems);

            for (int i = 1; i <= NumberOfItems; i++)
            {

                CatalogItem item = new CatalogItem
                {
                    Id = i,
                    Name = RandomString(8, alphanumeric),
                    Description = RandomString(8, alphanumeric),
                    Price = (decimal)(new Random().NextDouble() * 100),
                    PictureFileName = "",
                    PictureUri = "",
                    CatalogTypeId = i,

                    CatalogType = new CatalogType
                    {
                        Id = i,
                        Type = RandomString(8, alphanumeric)
                    },

                    CatalogBrandId = i,

                    CatalogBrand = new CatalogBrand
                    {
                        Id = i,
                        Brand = RandomString(8, alphanumeric)
                    },

                    AvailableStock = new Random().Next(Constants.MIN_STOCK_QTY, Constants.MAX_STOCK_QTY),
                    RestockThreshold = Constants.MIN_STOCK_QTY,
                    MaxStockThreshold = Constants.MAX_STOCK_QTY,
                    OnReorder = false


                };

                items.Add(item);

            }

            return items;
        }

        public static List<BasketItem> GenerateBasketItems(int NumberOfItems)
        {
            List<BasketItem> items = new List<BasketItem>(NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {

                BasketItem item = new BasketItem();

                item.ProductId = i;
                item.ProductName = RandomString(8, alphanumeric);

                item.UnitPrice = (decimal)(new Random().NextDouble() * 100);
                item.OldUnitPrice = item.UnitPrice;
                item.Quantity = 10; 
                item.PictureUrl = "";

                items.Add(item);

            }

            return items;
        }

        public static List<BasketItem> GenerateBasketForExistingItems(int NumberOfItems, List<CatalogItem> catalogItems)
        {
            List<BasketItem> basket = new List<BasketItem>(NumberOfItems);

            for (int i = 0; i < NumberOfItems; i++)
            {
                CatalogItem catalogItem = catalogItems[new Random().Next(catalogItems.Count)];
                BasketItem item = new BasketItem();

                item.ProductId = catalogItem.Id;
                item.ProductName = catalogItem.Name;

                item.UnitPrice = catalogItem.Price;
                item.OldUnitPrice = catalogItem.Price;
                item.Quantity = new Random().Next(1,catalogItem.AvailableStock);
                item.PictureUrl = null;

                basket.Add(item);

            }

            return basket;
        }

        public static List<ApplicationUser> GenerateCustomers(int NumberOfCustomers)
        {

            List<ApplicationUser> users = new List<ApplicationUser>(NumberOfCustomers);

            for (int i = 0; i < NumberOfCustomers; i++)
            {

                ApplicationUser user = new ApplicationUser();
                user.CardNumber = RandomString(8, numbers);
                user.SecurityNumber = RandomString(8, numbers);
                user.CardExpiration = DateTime.Now.AddYears(10);

                user.CardHolderName = RandomString(8, alphanumeric);
                user.CardType = new Random().Next(0, 2);

                user.Street = RandomString(8, alphanumeric);
                user.City = RandomString(8, alphanumeric);
                user.State = RandomString(8, alphanumeric);
                user.Country = RandomString(8, alphanumeric);
                user.ZipCode = RandomString(8, numbers);
                user.Name = RandomString(8, alphanumeric);
                user.LastName = RandomString(8, alphanumeric);

                users.Add(user);
            }


            return users;
        }
        
        public static List<string> GenerateGuids(int numberOfIds) {
            List<string> guids = new List<string>();
            for (int i = 0; i < numberOfIds; i++) {
                guids.Add(Guid.NewGuid().ToString());
            }
            return guids;
        }
        private static string RandomString(int length, string chars)
        {
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
