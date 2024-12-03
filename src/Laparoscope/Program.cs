using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Identity.Web.Resource;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Prometheus;
using Laparoscope.Workers;

namespace Laparoscope
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add and load configuration sources.
#pragma warning disable ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration
            bool didAzAppConfig = false;
            string configString = String.Empty;
            builder.Host.ConfigureAppConfiguration((hostingContext, config) => {
                config.Sources.Clear();

                var env = hostingContext.HostingEnvironment;
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }

                // NOTE: set the connection string value in an environment variable or appsettings json file with key: AppConfigConnectionString
                configString = builder.Configuration.GetValue<string>("AppConfigConnectionString");
                if (!String.IsNullOrEmpty(configString))
                {
                    config.AddAzureAppConfiguration(options => {
                        options.Connect(configString);
                    });
                    didAzAppConfig = true;
                }
            });
#pragma warning restore ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration

            // Azure AD Auth OIDC
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
            builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd");

            // Use forwarded headers for hosting behind a proxy
            builder.Services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });


            // Logging
            builder.Logging.AddConsole();

            builder.Services.AddRazorPages(options => {
                options.Conventions.AuthorizeFolder("/Server");
                options.Conventions.AuthorizeFolder("/Admin");
            })
                .AddRazorRuntimeCompilation()
                .AddMicrosoftIdentityUI();

            builder.Services.AddHostedService<HashSyncMetricCollector>();
            builder.Services.AddHostedService<ConnectorStatisticsMetricCollector>();

            builder.Services.AddControllers().AddJsonOptions(options => {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(
                opt => opt.ResolveConflictingActions(a => a.First()));

            builder.Services.AddHttpLogging(options => { });

            var app = builder.Build();


            ILogger logger = app.Logger;

            if (app.Configuration.GetValue<bool>("ForceHttpsScheme", false))
            {
                logger.LogInformation("Enforcing https scheme.");
                app.Use((context, next) => {
                    context.Request.Scheme = "https";
                    return next(context);
                });
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders();
            app.UseHttpsRedirection();

            app.UseHttpLogging();
            app.UseStaticFiles();

            app.UseAuthentication();
            
            app.MapControllers();

            app.UseRouting();
            app.ConfigureAuthorizationFilters();
            app.UseAuthorization();
            app.UseMetricServer();

            app.UseSwagger();
            //app.UseSwaggerUI();

            app.MapRazorPages();
            app.MapControllers();

            app.Run();
        }
    }
}
