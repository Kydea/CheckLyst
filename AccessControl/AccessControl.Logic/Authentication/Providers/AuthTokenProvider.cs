using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using NLog;

namespace CheckListManager.AccessControl.Logic.Authentication.Providers
{
	public class AuthTokenProvider : OAuthAuthorizationServerProvider
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ILifetimeScope _parentScope;

		public AuthTokenProvider(ISharingLifetimeScope parentScope)
		{
			_parentScope = parentScope.RootLifetimeScope;
		}

		public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			try
			{
				using (var scope = _parentScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
				{
					var form = await context.Request.ReadFormAsync();
					long? companyId = null;
					if (long.TryParse(form["companyId"], out var i))
						companyId = i;

					var userService = scope.Resolve<IUserService>();
					var user = await userService.AuthenticateAsync(companyId, context.UserName, context.Password);

                    var identity = CreateIdentity(context.Options.AuthenticationType, user);

                    var properties = new AuthenticationProperties(CreateProperties(user))
                    {
                        AllowRefresh = true
                    };

					var ticket = new AuthenticationTicket(identity, properties);

					context.Validated(ticket);
				}
			}
			catch (AuthenticationException exception)
			{
				Logger.Debug(exception, $"Error during login ({exception.ErrorCode})");
				context.SetError("invalid_grant", "The user name or password are incorrect.");
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "Error during login");
				context.SetError("invalid_grant", "The user name or password are incorrect.");
			}
		}

		public override Task TokenEndpoint(OAuthTokenEndpointContext context)
		{
			foreach (var property in context.Properties.Dictionary)
				context.AdditionalResponseParameters.Add(property.Key, property.Value);

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

		private static ClaimsIdentity CreateIdentity(string authenticationType, Users user)
		{
			var identity = new ClaimsIdentity(authenticationType);
			identity.AddClaim(new Claim(ClaimTypes.Version, DefaultIdentity.Version));
			identity.AddClaim(new Claim(ClaimTypes.Sid, user.Code));
			identity.AddClaim(new Claim(ClaimTypes.Name, user.Description));
			identity.AddClaim(new Claim("CompanyId", user.Company.ToString(), ClaimValueTypes.Integer));

            var Claims = new HashSet<string>();

            foreach (var function in user.UserFunctions) //user functions
            {
                if (function.Department != null)
                    Claims.Add($"{function.FunctionGroup}#{function.Function}#{function.Department.ToString() ?? string.Empty}#{function.PrototypeType?.ToString() ?? string.Empty}");
                Claims.Add($"{function.FunctionGroup}#{function.Function}");
                Claims.Add($"FunctionGroup#{function.FunctionGroup}");
            }

            foreach (var role in user.UserRoles) //user roles
                foreach (var function in role.Roles.RoleFunctions)
                {
                    if (role.Department != null)
                        Claims.Add($"{function.FunctionGroup}#{function.Function}#{role.Department.ToString() ?? string.Empty}#{role.PrototypeType?.ToString() ?? string.Empty}");
                    Claims.Add($"{function.FunctionGroup}#{function.Function}");
                    Claims.Add($"FunctionGroup#{function.FunctionGroup}");
                }

            foreach (var function in user.UserGroups.SelectMany(ug => ug.Groups.GroupFunctions)) //group functions
            {
                if (function.Department != null)
                    Claims.Add($"{function.FunctionGroup}#{function.Function}#{function.Department.ToString() ?? string.Empty}#{function.PrototypeType?.ToString() ?? string.Empty}");
                Claims.Add($"{function.FunctionGroup}#{function.Function}");
                Claims.Add($"FunctionGroup#{function.FunctionGroup}");
            }

            foreach (var role in user.UserGroups.SelectMany(ug => ug.Groups.GroupRoles)) //group roles
                foreach (var function in role.Roles.RoleFunctions)
                {
                    if (role.Department != null)
                        Claims.Add($"{function.FunctionGroup}#{function.Function}#{role.Department.ToString() ?? string.Empty}#{role.PrototypeType?.ToString() ?? string.Empty}");
                    Claims.Add($"{function.FunctionGroup}#{function.Function}");
                    Claims.Add($"FunctionGroup#{function.FunctionGroup}");
                }

            /*
            foreach (var level in user.UserLevels)
            {
                if (level.InstanceWorkFlowLevel != null)
                    Claims.Add($"InstanceWorkFlowLevel#{level.Department?.ToString() ?? string.Empty}#{level.PrototypeType?.ToString() ?? string.Empty}#{level.InstanceWorkFlowLevel}");
                if (level.PrototypeWorkFlowLevel != null)
                    Claims.Add($"PrototypeWorkFlowLevel#{level.Department?.ToString() ?? string.Empty}#{level.PrototypeType?.ToString() ?? string.Empty}#{level.PrototypeWorkFlowLevel}");
                if (level.SignatureLevel != null)
                    Claims.Add($"SignatureLevel#{level.Department?.ToString() ?? string.Empty}#{level.PrototypeType?.ToString() ?? string.Empty}#{level.SignatureLevel}");
            }
            */

            foreach (var Claim in Claims)
                identity.AddClaim(new Claim(ClaimTypes.Role, Claim));

            return identity;
		}

		private static Dictionary<string, string> CreateProperties(Users user)
		{
			return new Dictionary<string, string>
			{
				{ "company", user.Company.ToString() },
                { "decimalSeparator", user.Companies.SettingValues.SingleOrDefault(c => c.Setting == "DecimalSeparator")?.Value ?? "*" },
                { "code", user.Code },
				{ "name", user.Description },
                { "timeZone", user.Companies.TimeZone },
                { "roles", string.Join(",", user.UserFunctions.Select(function => $"{function.FunctionGroup}#{function.Function}#{function.Department?.ToString() ?? string.Empty}#{function.PrototypeType?.ToString() ?? string.Empty}")) }
            };
		}
	}
}