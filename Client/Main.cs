//using Orleans;
using System.Threading.Tasks;
using Client.UseCases.eShop;

namespace Client
{
    public class Program
    {


        public static int Main(string[] args)
        {
            EShopUseCase eShopUseCase = new EShopUseCase(new EShopDefaultConfig());
            eShopUseCase.Init();

            return 0;
        }

        private static async Task<int> RunMainAsync()
        {


            // TODO bulk data ingestor grain... maybe not necessary now, just a thread pool with a thread per microservice....

            // setup rabbitmq client after generating the data

            EShopUseCase eShopUseCase = new EShopUseCase(new EShopDefaultConfig());
            eShopUseCase.Init();


            return 0;
        }

        // Orleans for now we wont use
        /*public static async Task<IClusterClient> ConnectClient()
        {
            using var client = new ClientBuilder()
                                .UseLocalhostClustering()
                                //.ConfigureLogging(logging => logging.AddConsole())
                                .Build();
            await client.Connect();
            return client;
        }*/

    }

}
