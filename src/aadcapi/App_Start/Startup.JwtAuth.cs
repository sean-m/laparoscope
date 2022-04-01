
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
			string[] audiences = new string[] { Globals.RedirectUri, Globals.ApiUri };
			string issuer = Globals.Issuer;

			var signingKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(secret));

			var keyResolver = new OpenIdConnectSigningKeyResolver(authority);

			var options = new JwtBearerAuthenticationOptions
			{
				AuthenticationMode = AuthenticationMode.Active,
				AllowedAudiences = audiences,
				TokenValidationParameters = new TokenValidationParameters
				{
					RequireExpirationTime = true,
					ValidateLifetime = true,
					RequireSignedTokens = true,

					ValidIssuer = issuer,
					ValidateIssuer = true,
					ValidateIssuerSigningKey = true,

					ValidAudiences = audiences,
					ValidateAudience = true,
					
					IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => keyResolver.GetSigningKey(kid)
				},
			};

			app.UseJwtBearerAuthentication(options);
		}
	}
}