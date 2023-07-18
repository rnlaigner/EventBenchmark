﻿using Common.Workload.Delivery;
using Common.Workload.Metrics;

namespace Grains.WorkerInterfaces
{
	public interface IDeliveryProxy : IGrainWithIntegerKey
    {
        Task Init(DeliveryWorkerConfig config);
        Task<List<Latency>> Collect(DateTime finishTime);
	}
}