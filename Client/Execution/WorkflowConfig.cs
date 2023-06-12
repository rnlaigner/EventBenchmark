﻿using System;
namespace Client.Execution
{
	public class WorkflowConfig
	{
        // step
        // check whether the microservices are all active before starting the workflow
        public bool healthCheck = true;

        public string healthCheckEndpoint = "/health";

        // step
        public bool dataLoad = true;

        // step
        public bool ingestion = true;

        // step
        public bool transactionSubmission = true;

        // in future, constraint checking

        // whether should read events generated by microservices
        public bool pubsubEnabled = false;

        // submit requests to clean data created as part of the transaction submission e.g., orders, payments, shipments, carts, etc
        public bool cleanup = false;

        public string cleanupEndpoint = "/cleanup";
    }
}

