using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace PoseidonTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            PoseidonLogic.PoseidonManager manager = new PoseidonLogic.PoseidonManager();
            manager.Begin();

            Console.WriteLine("Press 'x' to exit");
            while (Console.ReadKey().Key != ConsoleKey.X) { }

            manager.Dispose();            
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
        }
    }
}
