using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Identity.Web.Resource;
using Microsoft.Extensions.Logging;

namespace Laparoscope
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            .AddMicrosoftIdentityUI();



            builder.Services.AddControllers();
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
            app.UseAuthorization();

            app.MapControllers();

            app.UseRouting();

            app.UseAuthorization();


            app.UseSwagger();
            //app.UseSwaggerUI();

            app.MapRazorPages();
            app.MapControllers();

            app.Run();

        }
    }
}
