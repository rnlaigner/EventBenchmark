using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using Client.UseCases;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace Client.RabbitMQ
{
    public class RabbitMQConnector
    {
        private static RabbitMQConnector instance = null;
        private static IConnection connection = null;
        private static IModel RabbitMqChannel = null;

        private static String queueName;
        private static String exchangeName = "EventBenchmark";
        public RabbitMQConnector() {
            
        }

        public static RabbitMQConnector getInstance()
        {
            if (instance == null)
                instance = new RabbitMQConnector();

            return instance;
        }

        public void connect() {
            ConnectionFactory factory = new ConnectionFactory();
            // "guest"/"guest" by default, limited to localhost connections
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "localhost";
            factory.Port = 5672;

            connection = factory.CreateConnection();
            Console.WriteLine($"succesfully connected to RabbitMQ at {DateTime.Now}");
            RabbitMqChannel = setExchangeConfiguration();
            Console.WriteLine($"succesfully created the excahnge and queue {exchangeName}");

            Console.WriteLine("Running EventBenchmark");
            Console.WriteLine($"Application {UseCases.Constants.APPLICATION}");
            Console.WriteLine($"Type {UseCases.Constants.TYPE}");
            Console.WriteLine("Check that the application is rechable swagger endpoints. DB clean, rabbit queue.");
            Console.WriteLine("press any key to contine the process...");
            Console.ReadKey();

        }

        public void closeConnection()  {
            connection.Close();
        }


        private IModel setExchangeConfiguration() {
            IModel channel = connection.CreateModel();
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
        
            queueName = channel.QueueDeclare(exchangeName, false, false, false, null).QueueName;  //RabbitMQ assigns and sets a non durable queue.

            // Listen to event of interest
            if (UseCases.Constants.APPLICATION.Equals(UseCases.Constants.CONTAINER))
                {
                //Basket
                channel.QueueBind(queueName, "eshop_event_bus", "OrderStartedIntegrationEvent");
                channel.QueueBind(queueName, "eshop_event_bus", "ProductPriceChangedIntegrationEvent");

                //Catalog
                channel.QueueBind(queueName, "eshop_event_bus", "OrderStatusChangedToAwaitingValidationIntegrationEvent");
                channel.QueueBind(queueName, "eshop_event_bus", "OrderStatusChangedToPaidIntegrationEvent");

                //Ordering
                channel.QueueBind(queueName, "eshop_event_bus", "GracePeriodConfirmedIntegrationEvent");
                channel.QueueBind(queueName, "eshop_event_bus", "OrderPaymentFailedIntegrationEvent");
                channel.QueueBind(queueName, "eshop_event_bus", "OrderPaymentSucceededIntegrationEvent");
                channel.QueueBind(queueName, "eshop_event_bus", "OrderStockConfirmedIntegrationEvent");
                channel.QueueBind(queueName, "eshop_event_bus", "OrderStockRejectedIntegrationEvent");
                channel.QueueBind(queueName, "eshop_event_bus", "UserCheckoutAcceptedIntegrationEvent");

                //Payment
                channel.QueueBind(queueName, "eshop_event_bus", "OrderStatusChangedToStockConfirmedIntegrationEvent");
            }
            else {
                if (UseCases.Constants.TYPE.Equals(UseCases.Constants.ENFORCED))
                {
                    channel.QueueBind(queueName, "ProductPriceChangedIntegrationEvent", "ProductPriceChangedIntegrationEvent");
                    channel.QueueBind(queueName, "ProductRemovedIntegrationEvent", "ProductRemovedIntegrationEvent");
                }

                channel.QueueBind(queueName, "OrderPaymentFailedIntegrationEvent", "OrderPaymentFailedIntegrationEvent");
                channel.QueueBind(queueName, "OrderPaymentSucceededIntegrationEvent", "OrderPaymentSucceededIntegrationEvent");
                channel.QueueBind(queueName, "OrderStatusChangedToAwaitingStockValidationIntegrationEvent", "OrderStatusChangedToAwaitingStockValidationIntegrationEvent");
                channel.QueueBind(queueName, "OrderStatusChangedToCancelledIntegrationEvent", "OrderStatusChangedToCancelledIntegrationEvent");
                channel.QueueBind(queueName, "OrderStatusChangedToPaidIntegrationEvent", "OrderStatusChangedToPaidIntegrationEvent");
                channel.QueueBind(queueName, "OrderStatusChangedToShippedIntegrationEvent", "OrderStatusChangedToShippedIntegrationEvent");
                channel.QueueBind(queueName, "OrderStatusChangedToSubmittedIntegrationEvent", "OrderStatusChangedToSubmittedIntegrationEvent");
                channel.QueueBind(queueName, "OrderStatusChangedToValidatedIntegrationEvent", "OrderStatusChangedToValidatedIntegrationEvent");
                channel.QueueBind(queueName, "OrderStockConfirmedIntegrationEvent", "OrderStockConfirmedIntegrationEvent");
                channel.QueueBind(queueName, "OrderStockRejectedIntegrationEvent", "OrderStockRejectedIntegrationEvent");
                channel.QueueBind(queueName, "UserCheckoutAcceptedIntegrationEvent", "UserCheckoutAcceptedIntegrationEvent");

            }
            
            
            Console.WriteLine($"[*] Queue {queueName} is bound and listening for messages.");
            Console.WriteLine("press any key to contine the process...");
            Console.ReadKey();
            return channel;
        }

        // consuming will be done in the rabbit mq
        // there we will look in the messages
        private void consumeEvents(IModel channel) { }

    }
}
