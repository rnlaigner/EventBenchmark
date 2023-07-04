﻿namespace Common.Workload.Metrics
{
	public record Latency
	(
        int tid,
		TransactionType type,
		int period
	);
}

