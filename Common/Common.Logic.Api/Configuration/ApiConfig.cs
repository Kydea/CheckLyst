using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using CheckListManager.Common.Logic.Api.Authentication;
using CheckListManager.Common.Logic.Api.Filters;
using CheckListManager.Common.Logic.Configuration;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;

namespace CheckListManager.Common.Logic.Api.Configuration
{
	public abstract class ApiConfig : ConfigBase
	{
		public bool LoadAssemblies { private get; set; }

		protected override bool UseInstancePerRequest => true;

		protected override void OnCreateBuilder(ContainerBuilder builder)
		{
			base.OnCreateBuilder(builder);
			builder.RegisterApiControllers(Assemblies);
		}

		protected override Assembly[] GetAssemblies()
		{
			return LoadAssemblies ? BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray() : base.GetAssemblies();
		}
	}

	public static class ApiConfigExtensions
	{
		public static void Configure(this HttpConfiguration configuration, ApiConfig apiConfig)
		{
			ConfigureDependencyInjection(configuration, apiConfig.CreateBuilder());

			configuration.Services.Replace(typeof(IExceptionHandler), new DefaultExceptionHandler());

			configuration.SuppressDefaultHostAuthentication();
			configuration.Filters.Add(new AuthorizeAttribute());
			configuration.Filters.Add(new DefaultAuthenticationFilter(OAuthDefaults.AuthenticationType));
			configuration.Filters.Add(new ArgumentNullValidationActionFilterAttribute());
			configuration.Filters.Add(new ModelValidationActionFilterAttribute());

			configuration.MapHttpAttributeRoutes();

			configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			configuration.EnableCors(new EnableCorsAttribute("https://clm.matematici-aws.com", "*", "*"));
		}

		private static void ConfigureDependencyInjection(HttpConfiguration configuration, ContainerBuilder builder)
		{
			GlobalConfiguration.Configuration.DependencyResolver =
				configuration.DependencyResolver
					= new AutofacWebApiDependencyResolver(builder.Build());
		}
	}
}
