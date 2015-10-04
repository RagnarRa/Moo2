using System.Collections.Generic;
using System.IdentityModel.Tokens;
using API;
using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityServer.AccessTokenValidation;

[assembly: OwinStartup(typeof(Startup))]
namespace API
{
	/// <summary>
	/// A startup class for authentication
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// Configuration of authentication
		/// </summary>
		/// <param name="app"></param>
		public void Configuration(IAppBuilder app)
		{
			// Note: this line is necessary if we use ValidationMode.Local
			// (see below). If not, we would not receive the "sub" claim
			// from the token. The alternative from using the Local
			// validation option is to use Endpoint validation, in which
			// case the IdentityServer would be contacted in each 
			// request we receive.
			JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

			app.UseIdentityServerBearerTokenAuthentication(
				new IdentityServerBearerTokenAuthenticationOptions
				{
					Authority      = "http://dispatch.ru.is/auth/core",
					RequiredScopes    = new[] { "read" },
					ValidationMode    = ValidationMode.Local
				});
		}
	}
}
