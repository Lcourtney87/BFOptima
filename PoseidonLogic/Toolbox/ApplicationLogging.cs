using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoseidonLogic
{
    public static class ApplicationLogging
    {
        public static ILoggerFactory factory { get; } = LoggerFactory.Create(builder => builder.AddConsole());
        public static ILogger CreateLogger<T>() =>
          factory.CreateLogger<T>();
    }
}
