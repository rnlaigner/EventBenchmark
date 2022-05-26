using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Client.UseCases.eShopDapr.TransactionInput;
using Common.Entities.eShopDapr;

namespace Client.UseCases.eShopDapr.Transactions
{
    /**
     * This class models a user interacting with the webshop
     * Adding several items accross a ttime span
     * And later checking out the order
     */
    public class Checkout 
    {

        private readonly CheckoutTransactionInput input;
        private readonly HttpClient client;

        // do we have an average timespan between requests?
        public bool Waitable { get; set; }


        public Checkout(HttpClient client, CheckoutTransactionInput input)
        {
            this.input = input;
            this.client = client;
            this.Waitable = true;
        }

        public async Task<HttpResponseMessage> Run(int userId)// only parameter not shared across input
        {

           //Task[] listWaitAddCart = new Task[1];

           var payload = new BasketCheckout()  {
                City = input.Users[userId].City,
                Street = input.Users[userId].Street,
                Country = input.Users[userId].Country,
                ZipCode = input.Users[userId].ZipCode,
                CardNumber = input.Users[userId].CardNumber,
                CardHolderName = input.Users[userId].CardHolderName,
                CardExpiration = input.Users[userId].CardExpiration,
                CardSecurityNumber = input.Users[userId].SecurityNumber,
                CardTypeId = input.Users[userId].CardType,
                Buyer = input.Users[userId].Name,
                UserId = input.BasketIds[userId],
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("x-requestid", input.BasketIds[userId]);

            // now checkout
            Console.WriteLine();
            Console.WriteLine("CHECKOUT");
            Console.WriteLine(content);
            Console.WriteLine();
            return await client.PostAsync(input.CartUrl, content);

        }

    }
}
