using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace PoseidonTest
{
    class Program
    {
        static void Main(string[] args)
        {
            PoseidonLogic.PoseidonManager manager = new PoseidonLogic.PoseidonManager();
            manager.Begin();

            Console.WriteLine("Press 'x' to exit");
            while (Console.ReadKey().Key != ConsoleKey.X) { }

            manager.Dispose();            
        }
    }
}
