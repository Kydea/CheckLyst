using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Cors;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Integration.WebApi;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Authentication;
using CheckListManager.Common.Logic.Api.Filters;
using CheckListManager.Common.Logic.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Owin;

using ServiceClient = CheckListManager.Common.Core.Web.Network.ServiceClient;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Common.Logic.Api.Formatters;

namespace CheckListManager.Common.Logic.Api.Configuration
{
	public abstract class ApiConfig : ConfigBase
	{
		private readonly HttpConfiguration _configuration;

		protected IContainer Container { get; private set; }

		protected ApiConfig()
		{
			_configuration = new HttpConfiguration();
			UseInstancePerRequest = true;
		}

		public bool LoadAssemblies { private get; set; }
		protected bool ConfigureOwinProvider { private get; set; }

		protected override void OnCreateBuilder(ContainerBuilder builder)
		{
			base.OnCreateBuilder(builder);
			builder.RegisterApiControllers(Assemblies);
			builder.RegisterHttpRequestMessage(_configuration);

			builder.Register(ConfigureServiceClient)
				.As<IServiceClient>()
				.As<ServiceClient>()
				.InstancePerDependency();
		}

		private static ServiceClient ConfigureServiceClient(IComponentContext context, IEnumerable<Parameter> parameters)
		{
			var baseUrl = GetServiceBaseUrl(context, parameters);
			return new ServiceClient(baseUrl, context.Resolve<HttpRequestMessage>());
		}

		protected override Assembly[] GetAssemblies()
		{
			return LoadAssemblies ? BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray() : base.GetAssemblies();
		}

		protected void Configure(IAppBuilder app)
		{
			app.UseCors(new CorsOptions
			{
				PolicyProvider = new CorsPolicyProvider
				{
					PolicyResolver = context => Task.FromResult(new CorsPolicy
					{
						AllowAnyOrigin = true,
						AllowAnyHeader = true,
						AllowAnyMethod = true,
						PreflightMaxAge = 1000
					})
				}
			});

			app.Use((context, next) =>
			{
				if (context.Request.Headers.Any(k => k.Key.Contains("Origin")) && "OPTIONS".Equals(context.Request.Method, StringComparison.InvariantCulture))
				{
					context.Response.StatusCode = 200;
					return context.Response.WriteAsync("handled");
				}

				return next.Invoke();
			});
			ConfigureWebApi();
			ConfigureAuth(app);
			app.UseWebApi(_configuration);
		}

		private void ConfigureWebApi()
		{
			Container = CreateBuilder().Build();

			_configuration.DependencyResolver
				= new AutofacWebApiDependencyResolver(Container);

			_configuration.Services.Replace(typeof(IExceptionHandler), new DefaultExceptionHandler());

			_configuration.SuppressDefaultHostAuthentication();
			_configuration.Filters.Add(new AuthorizeAttribute());
			_configuration.Filters.Add(new DefaultAuthenticationFilter(OAuthDefaults.AuthenticationType));
			_configuration.Filters.Add(new ArgumentNullValidationActionFilterAttribute());
			_configuration.Filters.Add(new ModelValidationActionFilterAttribute());

			_configuration.MapHttpAttributeRoutes();

			_configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new CheckListJsonConverter(Container));

            OnConfigureWebApi(_configuration);
		}

		protected virtual void OnConfigureWebApi(HttpConfiguration configuration)
		{
			
		}

		private void ConfigureAuth(IAppBuilder app)
		{
			if (ConfigureOwinProvider)
				ConfigureAuthProvider(app);
			else
				ConfigureAuthToken(app);
		}

		private static void ConfigureAuthToken(IAppBuilder app)
		{
			app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
		}

		private void ConfigureAuthProvider(IAppBuilder app)
		{
			var parentScope =
				(ILifetimeScope)_configuration.DependencyResolver.GetService(typeof(ILifetimeScope));
			using (var scope = parentScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, builder => OnConfigureAuthProvider(app, builder)))
			{
				var oAuthOptions = new OAuthAuthorizationServerOptions
				{
					TokenEndpointPath = new PathString("/Access/Token"),
					Provider = scope.Resolve<OAuthAuthorizationServerProvider>(),
					AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30), //TODO load from configuration
					RefreshTokenProvider = scope.Resolve<IAuthenticationTokenProvider>(),
					AllowInsecureHttp = true
				};

				app.UseOAuthBearerTokens(oAuthOptions);
			}
		}

		protected virtual void OnConfigureAuthProvider(IAppBuilder app, ContainerBuilder builder)
		{
			
		}
	}
}
