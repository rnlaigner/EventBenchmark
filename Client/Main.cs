using Client.UseCases.eShop;
using Client.UseCases.eShopDapr;

namespace Client
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // WHEN RUNNING CONTAINERS
            EShopUseCase eShopUseCase = new EShopUseCase(new EShopDefaultConfig());
            eShopUseCase.Init();

            // WHEN RUNNING DAPR 
            //EShopDaprUseCase eShopDaprUseCase = new EShopDaprUseCase(new EShopDaprDefaultConfig());
            //eShopDaprUseCase.Init();

            return 0;
        }

    }

}
