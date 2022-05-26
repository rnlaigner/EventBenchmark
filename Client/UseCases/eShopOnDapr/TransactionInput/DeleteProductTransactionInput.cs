using System.Collections.Generic;
using Common.Entities.eShopDapr;

namespace Client.UseCases.eShopDapr.TransactionInput
{
    public class DeleteProductTransactionInput : IInput
    {

        public int NumTotalItems { get; set; }
        public string CatalogUrl { get; set; }
        // Filling data about the items present in the database
        public List<CatalogItem> Items { get; set; }

    }
}
