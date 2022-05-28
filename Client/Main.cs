using System;
using Client.UseCases;
using Client.UseCases.eShop;
using Client.UseCases.eShopDapr;

namespace Client
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // connect to rabbit MQ for all
            RabbitMQ.RabbitMQConnector connector = RabbitMQ.RabbitMQConnector.getInstance();
            connector.connect();

            // WHEN RUNNING CONTAINERS
            if (Constants.APPLICATION.Equals(Constants.CONTAINER)) {
                EShopUseCase eShopUseCase = new EShopUseCase(new EShopDefaultConfig(), Constants.SIMPLE_SUCESS_FLOW);
                eShopUseCase.Init();
            }
            // WHEN RUNNING DAPR 
            else if (Constants.APPLICATION.Equals(Constants.DAPR)) {
                EShopDaprUseCase eShopDaprUseCase = new EShopDaprUseCase(new EShopDaprDefaultConfig(), Constants.SIMPLE_SUCESS_FLOW);
                eShopDaprUseCase.Init();
            } else {
                Console.WriteLine("No setup selected");
            }
            connector.closeConnection();
            return 0;
        }   
    }

}
