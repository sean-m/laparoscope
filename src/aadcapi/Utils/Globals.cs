using SMM.Automation;
using SMM.Helper;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace aadcapi.Utils
{
    public static class Globals
    {
        public const string ConsumerTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";
        public const string IssuerClaim = "iss";
        public const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string MicrosoftGraphGroupsApi = "https://graph.microsoft.com/v1.0/groups";
        public const string MicrosoftGraphUsersApi = "https://graph.microsoft.com/v1.0/users";
        public const string AdminConsentFormat = "https://login.microsoftonline.com/{0}/adminconsent?client_id={1}&state={2}&redirect_uri={3}";
        public const string BasicSignInScopes = "openid profile";
        public const string NameClaimType = "name";

        /// <summary>
        /// The Client ID is used by the application to uniquely identify itself to Azure AD.
        /// </summary>
        public static string ClientId { get => ConfigurationManager.AppSettings["ida:ClientId"]; }

        /// <summary>
        /// The ClientSecret is a credential used to authenticate the application to Azure AD.  Azure AD supports password and certificate credentials.
        /// </summary>
        public static string ClientSecret { get => ConfigurationManager.AppSettings["ida:ClientSecret"]; }

        /// <summary>
        /// The Post Logout Redirect Uri is the URL where the user will be redirected after they sign out.
        /// </summary>
        public static string PostLogoutRedirectUri { get => ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"]; }

        /// <summary>
        /// The TenantId is the DirectoryId of the Azure AD tenant being used in the sample
        /// </summary>
        public static string TenantId {
            get => ConfigurationManager.AppSettings["ida:TenantId"];
        }


        /// <summary>
        /// The AppId guid specified in the Azure AD app registration.
        /// </summary>
        public static string Issuer {
            get => $"https://sts.windows.net/{TenantId}/";
        }


        /// <summary>
        /// This must be in the allowed redirect uris in the app registration.
        /// </summary>
        public static string RedirectUri {
            get => ConfigurationManager.AppSettings["ida:RedirectUri"];
        }

        public static string ApiUri {
            get => ConfigurationManager.AppSettings["ida:ApiUri"];
        }

        /// <summary>
        /// This is exposed at the bottom of the global layout view.
        /// </summary>
        public static string AppName {
            get => ConfigurationManager.AppSettings["AppName"] ?? "Didn't fill out your AppName did you?";
        }

        /// <summary>
        /// This is shows as the login endpoint when configuring authentication in the Azure AD Portal.
        /// For a multi-tenant configured app, populate the Authority attribte in the web.config.
        /// </summary>
        public static string Authority { get => ConfigurationManager.AppSettings["ida:Authority"] ?? $"https://login.microsoftonline.com/{TenantId}/"; }

        /// <summary>
        /// Users in this specified role will be able to query and manage application settings at runtime via the api.
        /// </summary>
        public static string AuthorizedConfigRole { get => ConfigurationManager.AppSettings["ops:ConfigManagerRole"] ?? "Admin"; }


        /// <summary>
        /// Indicate AADC staging status. This variable is updated by a POST action to the /api/health controller.
        /// </summary>
        public static string Staging { get; set; } = GetStagingMode();

        /// <summary>
        /// This goes with the Staging property above. ^^ That one.
        /// </summary>
        /// <returns></returns>
        internal static string GetStagingMode()
        {
            using (var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncScheduler"))
            {
                runner.Run();

                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    return string.Empty;
                }

                var result = runner.Results.ToDict().FirstOrDefault();
                if (result.ContainsKey("StagingModeEnabled"))
                {
                    return result["StagingModeEnabled"].ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Time the running binary was linked. This is exposed via the /api/health controller 
        /// so operators can monitor the values across multiple servers to ensure the same application
        /// version is deployed across the fleet.
        /// </summary>
        public static string LinkerTimeUtc { get; private set; } = GetLinkerTime(Assembly.GetExecutingAssembly(), TimeZoneInfo.Utc).ToString("o");

        // Sourced from the comments of a lovely Jeff Atwood article: https://blog.codinghorror.com/determining-build-date-the-hard-way/
        // Jeff's example is in VB.net, C# port is by Joe Spivey. Thank them if you get a chance.
        private static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[1024];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 1024);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        /// <summary>
        /// Assmbly version also exposed via /api/health for the same reasons as LinkerTimeUtc.
        /// </summary>
        public static string AssemblyVersion { get; private set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}