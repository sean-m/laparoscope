using System.Configuration;

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
        public static string ClientId { get; } = ConfigurationManager.AppSettings["ida:ClientId"];

        /// <summary>
        /// The ClientSecret is a credential used to authenticate the application to Azure AD.  Azure AD supports password and certificate credentials.
        /// </summary>
        public static string ClientSecret { get; } = ConfigurationManager.AppSettings["ida:ClientSecret"];

        /// <summary>
        /// The Post Logout Redirect Uri is the URL where the user will be redirected after they sign out.
        /// </summary>
        public static string PostLogoutRedirectUri { get; } = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        /// <summary>
        /// The TenantId is the DirectoryId of the Azure AD tenant being used in the sample
        /// </summary>
        public static string TenantId {
            get;
        } = ConfigurationManager.AppSettings["ida:TenantId"];

        /// <summary>
        /// This must be in the allowed redirect uris in the app registration.
        /// </summary>
        public static string RedirectUri { 
            get; 
        } = ConfigurationManager.AppSettings["ida:RedirectUri"];

        /// <summary>
        /// This is exposed at the bottom of the global layout view.
        /// </summary>
        public static string AppName {
            get;
        } = ConfigurationManager.AppSettings["AppName"] ?? "Didn't fill out your AppName did you?";

        /// <summary>
        /// This is shows as the login endpoint when configuring authentication in the Azure AD Portal.
        /// For a multi-tenant configured app, populate the Authority attribte in the web.config.
        /// </summary>
        public static string Authority { get; } = ConfigurationManager.AppSettings["ida:Authority"] ??  $"https://login.microsoftonline.com/{TenantId}/";
    }
}