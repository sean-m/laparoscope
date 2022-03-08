
using aadcapi.Utils;
using Auth0.Owin;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Jwt;
using Owin;

namespace aadcapi
{
    public partial class Startup
	{
		private void ConfigureJwtAuth(IAppBuilder app) {
			
			app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

			app.UseCookieAuthentication(new CookieAuthenticationOptions { });

			string authority = Globals.Authority;
			string clientId = Globals.ClientId;
			string secret = Globals.ClientSecret;
			string audience = Globals.RedirectUri;

			var signingKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(secret));

			var keyResolver = new OpenIdConnectSigningKeyResolver(authority);

			var options = new JwtBearerAuthenticationOptions
			{
				AuthenticationMode = AuthenticationMode.Active,
				AllowedAudiences = new string[] { audience },
				TokenValidationParameters = new TokenValidationParameters
				{
					RequireExpirationTime = true,
					ValidateLifetime = true,
					RequireSignedTokens = false,
					ValidateIssuerSigningKey = false,
					ValidateIssuer = false,
					ValidateAudience = false,
					IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => keyResolver.GetSigningKey(kid)
				},
			};

			app.UseJwtBearerAuthentication(options);
		}
	}
}