using System.Collections.Generic;
using Common.Entities.eShop;


namespace Client.UseCases.eShop.TransactionInput
{
    public class StockReplenishmentTransactionInput : IInput
    {

        public int NumTotalItems { get; set; }
        public string CatalogUrl { get; set; }

        // Filling data about the items present in the database
        public List<CatalogItem> Items { get; set; }

    }
}
