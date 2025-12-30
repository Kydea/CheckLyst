using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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

        private IEnumerable<UserFunction> PrepareUserFunctions(Users user)
        {
            foreach (var function in user.UserFunctions) //user functions
                yield return new UserFunction { FunctionGroup = function.FunctionGroup, Function = function.Function, Department = function.Department, PrototypeType = function.PrototypeType };

            foreach (var role in user.UserRoles) //user roles
                foreach (var function in role.Roles.RoleFunctions)
                    yield return new UserFunction { FunctionGroup = function.FunctionGroup, Function = function.Function, Department = role.Department, PrototypeType = role.PrototypeType };

            foreach (var function in user.UserGroups.SelectMany(ug => ug.Groups.GroupFunctions)) //group functions
                yield return new UserFunction { FunctionGroup = function.FunctionGroup, Function = function.Function, Department = function.Department, PrototypeType = function.PrototypeType };

            foreach (var role in user.UserGroups.SelectMany(ug => ug.Groups.GroupRoles)) //group roles
                foreach (var function in role.Roles.RoleFunctions)
                    yield return new UserFunction { FunctionGroup = function.FunctionGroup, Function = function.Function, Department = role.Department, PrototypeType = role.PrototypeType };

            foreach (var level in user.UserLevels) //user levels
                yield return new UserFunction { Level = true, Department = level.Department, PrototypeType = level.PrototypeType, SignatureLevel = level.SignatureLevel, PrototypeWorkFlowLevel = level.PrototypeWorkFlowLevel, InstanceWorkFlowLevel = level.InstanceWorkFlowLevel };
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
                    var grants = PrepareUserFunctions(user).SelectMany(f => f.GetClaims()).Distinct().ToList();
                    if (user.ReportWorkFlowLevel != null) grants.Add($"ReportWorkFlowLevel####{user.ReportWorkFlowLevel}");
                    var identity = CreateIdentity(context.Options.AuthenticationType, user, grants);

                    var properties = new AuthenticationProperties(CreateProperties(user, grants))
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

        private static ClaimsIdentity CreateIdentity(string authenticationType, Users user, List<string> claims)
        {
            var identity = new ClaimsIdentity(authenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Version, DefaultIdentity.Version));
            identity.AddClaim(new Claim(ClaimTypes.Sid, user.Code));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Description));
            identity.AddClaim(new Claim("CompanyId", user.Company.ToString(), ClaimValueTypes.Integer));

            foreach (var claim in claims)
                identity.AddClaim(new Claim(ClaimTypes.Role, claim, ClaimValueTypes.Integer));

            return identity;
        }

        private static Dictionary<string, string> CreateProperties(Users user, List<string> claims)
        {
            return new Dictionary<string, string>
            {
                { "company", user.Company.ToString() },
                { "decimalSeparator", user.Companies.SettingValues.SingleOrDefault(c => c.Setting == "DecimalSeparator")?.Value ?? "*" },
                { "code", user.Code },
                { "name", user.Description },
                { "timeZone", user.TimeZone ?? user.Companies.TimeZone },
                { "companyTimeZone", user.Companies.TimeZone },
                { "roles", string.Join(",", claims.Select(c => c + '#')) }
            };
        }

        private class UserFunction
        {
            public string FunctionGroup { get; set; }
            public string Function { get; set; }
            public long? Department { get; set; }
            public long? PrototypeType { get; set; }
            public bool Level { get; set; }
            public string SignatureLevel { get; set; }
            public string PrototypeWorkFlowLevel { get; set; }
            public string InstanceWorkFlowLevel { get; set; }

            public IEnumerable<string> GetClaims()
            {
                if (Level)
                {
                    if (SignatureLevel != null)
                        yield return $"SignatureLevel##{Department?.ToString() ?? string.Empty}#{PrototypeType?.ToString() ?? string.Empty}#{SignatureLevel}";
                    if (PrototypeWorkFlowLevel != null)
                        yield return $"PrototypeWorkFlowLevel##{Department?.ToString() ?? string.Empty}#{PrototypeType?.ToString() ?? string.Empty}#{PrototypeWorkFlowLevel}";
                    if (InstanceWorkFlowLevel != null)
                        yield return $"InstanceWorkFlowLevel##{Department?.ToString() ?? string.Empty}#{PrototypeType?.ToString() ?? string.Empty}#{InstanceWorkFlowLevel}";
                }
                else
                {
                    if (Department != null)
                        yield return $"{FunctionGroup}#{Function}#{Department?.ToString() ?? string.Empty}#{PrototypeType?.ToString() ?? string.Empty}";
                    yield return $"{FunctionGroup}#{Function}";
                    yield return $"FunctionGroup#{FunctionGroup}";
                }
            }
        }
    }
}