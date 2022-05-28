using System;
namespace Client.UseCases
{
    public static class Constants
    {

        // application to benchmark and its type
        public const string APPLICATION = CONTAINER;
        public const string TYPE = BASE;

        // setup of constants for easier chekup
        public const string CONTAINER = "CONTAINER";
        public const string DAPR = "DAPR";

        public const string ENFORCED = "ENFORCED";
        public const string BASE = "BASE";

        // IP addresses
        public const string BASKET_IP = "localhost:5103";
        public const string CATALOG_IP = "localhost:5101";
        public const string ORDER_IP = "localhost:5102";

        // addresses of the endpoint to open
        //public const string RABBITMQ_ADDRESS = "http://localhost:15672/#/queues";
        //public const string SWAGGER = "http://localhost:5101/swagger/index.html";

        // Scenarios
        public const string REFERENTIAL_INTEGRITY = "REFERENTIAL_INTEGRITY";
        public const string NON_NEGATIVE_ENFORCEMENT = "NON_NEGATIVE_ENFORCEMENT";
        public const string EXACTLY_ONCE_PROCESSING = "EXACTLY_ONCE_PROCESSING";
        public const string PERFORMANCE = "PERFORMANCE";
        public const string SIMPLE_SUCESS_FLOW = "SIMPLE_SUCESS_FLOW";

    }
}
