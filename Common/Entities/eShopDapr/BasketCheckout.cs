using System;

namespace Common.Entities.eShopDapr
{
    public class BasketCheckout
    {
        public string City { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public string CardNumber { get; set; }

        public string CardHolderName { get; set; }

        public DateTime CardExpiration { get; set; }

        public string CardSecurityNumber { get; set; }

        public int CardTypeId { get; set; }

        public string Buyer { get; set; }

        // filled which was introduce as replacement of identity service
        // not safe practice but workabel solution
        // in the checkout user also shows which basket is his
        public string UserId { get; set; }
    }
}
