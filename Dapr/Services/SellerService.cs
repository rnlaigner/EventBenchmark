﻿using Common.Entities;
using Common.Workload;
using Common.Workload.Metrics;
using Daprr.Workers;

namespace Daprr.Services;

public sealed class SellerService : ISellerService
{

    private readonly Dictionary<int, SellerThread> sellers;

    public SellerService(Dictionary<int, SellerThread> sellers)
    {
        this.sellers = sellers;
    }

    public Product GetProduct(int sellerId, int idx) => sellers[sellerId].GetProduct(idx);

    public List<TransactionOutput> GetFinishedTransactions(int sellerId)
    {
        return sellers[sellerId].GetFinishedTransactions();
    }

    public List<TransactionIdentifier> GetSubmittedTransactions(int sellerId)
    {
        return sellers[sellerId].GetSubmittedTransactions();
    }

    public void Run(int sellerId, int tid, TransactionType type) => sellers[sellerId].Run(tid, type);

}

