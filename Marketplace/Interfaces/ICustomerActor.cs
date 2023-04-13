﻿using System;
using Common.Scenario.Entity;
using Marketplace.Infra;
using Orleans;
using System.Threading.Tasks;

namespace Marketplace.Interfaces
{
    public interface ICustomerActor : IGrainWithIntegerKey, SnapperActor
    {
        public Task<Customer> GetCustomer(long customerId);
        public Task NotifyPayment(long customerId, Order order = null, bool success = true);
        public Task NotifyDelivery(long customerId);

        // API
        public Task AddCustomer(Customer customer);
    }
}
