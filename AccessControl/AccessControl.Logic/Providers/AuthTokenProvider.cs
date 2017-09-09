using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Data;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using NLog;

namespace CheckListManager.AccessControl.Logic.Providers
{
    public class AuthTokenProvider : OAuthAuthorizationServerProvider
    {
	    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

	    private readonly IUserService _userService;

	    public AuthTokenProvider(IUserService userService)
	    {
		    _userService = userService;
	    }

	    public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
	        try
	        {
		        var user = await _userService.AuthenticateAsync(context.UserName, context.Password);

				var oAuthIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
		        oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Code));
		        oAuthIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Code));

		        var properties = CreateProperties(user);
		        var ticket = new AuthenticationTicket(oAuthIdentity, properties);
		        context.Validated(ticket);
			}
	        catch (AuthenticationException exception)
	        {
		        Logger.Debug(exception, $"Error during login ({exception.ErrorCode})");
		        context.SetError("invalid_grant", "The user name or password is incorrect.");
	        }
			catch (Exception exception)
	        {
		        Logger.Error(exception, "Error during login");
		        context.SetError("invalid_grant", "The user name or password is incorrect.");
	        }
        }

		public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (var property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
	        context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
	        context.Validated();
            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(Users user)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
				{ "company", user.Company.ToString() },
                { "code", user.Code } //TODO: add more
            };
            return new AuthenticationProperties(data);
        }
    }
}