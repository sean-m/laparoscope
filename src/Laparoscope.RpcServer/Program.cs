using Laparoscope.RpcServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMM.Helper;
using System.Linq;
using System.Text.RegularExpressions;

namespace App
{
    public static class Program {
        public static void Main(string[] args) {
            var builder = Host.CreateApplicationBuilder(args);
            Regex helpFlag = new Regex(@"(?i)(-h|--help|/\?)");
            if (args.Any(x => helpFlag.IsMatch(x.Trim().ToLower())))
            {
                var help = @"
Laparoscope.RpcServer
Arguments:
    --Logging [console | winevent]  * defaults to console if non specified
    --LogLevel [info | warn | critical | verbose]   * defaults to info
    -h | --help | /?    show this help
";
                System.Console.WriteLine(help);
                return;
            }

            var logType = builder.Configuration.GetValue<string>("Logging", "console");
            var logLevel = builder.Configuration.GetValue<string>("LogLevel", "info");


            builder.Services.AddLogging(option => {
                switch (logType.Trim().ToLower())
                {
                    case "winevent":
                        option.AddEventLog(settings =>
                        {
                            settings.LogName = "Application";
                            settings.SourceName = "Laparoscope.RpcServer";
                        });
                        break;
                    default:
                        option.AddConsole();
                        break;
                }
                option.SetMinimumLevel(parseLevel(logLevel));
            });
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