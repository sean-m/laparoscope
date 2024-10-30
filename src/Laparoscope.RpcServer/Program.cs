using Laparoscope.RpcServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using SMM.Helper;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace App
{
    public static class Program {
        public static void Main(string[] args) {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            
            builder.Services.AddWindowsService();
            builder.Services.AddLogging();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }

        static LogLevel parseLevel(string level) {
            switch (level.Trim().ToLower()) {
                case "critical":
                    return LogLevel.Critical;
                case "verbos":
                    return LogLevel.Debug;
                case "warn":
                    return LogLevel.Warning;
                default:
                    return LogLevel.Information;
            }
        }
    }
}