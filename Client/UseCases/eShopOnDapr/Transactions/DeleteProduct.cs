﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Client.UseCases.eShopDapr.TransactionInput;
using Common.YCSB;

namespace Client.UseCases.eShopDapr.Transactions
{
    public class DeleteProduct
    {

        private readonly NumberGenerator numberGenerator;
        private readonly DeleteProductTransactionInput input;
        private readonly HttpClient client;

        private readonly TimeSpan timeSpan;
        private readonly bool Waitable;

        private readonly CountdownEvent cte;

        public DeleteProduct(NumberGenerator numberGenerator, DeleteProductTransactionInput input)
        {
            this.numberGenerator = numberGenerator;
            this.input = input;
            this.client = new HttpClient();
            this.Waitable = false;
            this.cte = new CountdownEvent(0);
        }

        public DeleteProduct(NumberGenerator numberGenerator, DeleteProductTransactionInput input, TimeSpan timeSpan) : this(numberGenerator, input)
        {
            this.timeSpan = timeSpan;
            this.Waitable = true;
        }

        public async Task Run()
        {

            var random = new Random();

            // keep generating price updates until stopped


            while(cte.CurrentCount == 0)
            {

                int itemId = (int)numberGenerator.NextValue();

                // TODO fix payload
                HttpContent payload = null;

                await client.PostAsync(input.CatalogUrl, payload);


                if (Waitable) Thread.Sleep(timeSpan);

            }

            
        }

        public Task Close()
        {
            return Task.FromResult<bool>(cte.TryAddCount(1));
        }


    }
}
