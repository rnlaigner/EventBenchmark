using System.Collections.Generic;
using Common.Entities.eShop;

namespace Client.UseCases.eShop.TransactionInput
{
    public class CheckoutTransactionInput : IInput
    {

        public int MinNumItems { get; set; }
        public int MaxNumItems { get; set; }

        public int MinItemQty { get; set; }
        public int MaxItemQty { get; set; }

        public string CartUrl { get; set; }

        // Filling data about the customer
        public List<ApplicationUser> Users { get; set; }

        // Filling data about the basketId and will also be used for requestId
        public List<string> BasketIds { get; set; }

    }
}
