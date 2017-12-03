using Autofac;
using Autofac.Core;
using AutoMapper;
using CheckListManager.AccessControl.Logic.Authentication.Providers;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Configuration;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace CheckListManager.AccessControl.Api
{
    public class WebApiConfig : ApiConfig
    {
	    protected override void OnCreateMapper(IMapperConfigurationExpression mapperConfig)
	    {
		    base.OnCreateMapper(mapperConfig);

		    mapperConfig.CreateMap<UserRequest, Users>()
			    .ForMember(m => m.Password, opt => opt.Ignore());
	    }

	    protected override void OnConfigureAuthProvider(IAppBuilder app, ContainerBuilder builder)
	    {
		    builder.RegisterType<AuthTokenProvider>()
				.As<OAuthAuthorizationServerProvider>()
				.AsSelf()
				.SingleInstance();
		    builder.RegisterType<RefreshTokenProvider>()
				.As<IAuthenticationTokenProvider>()
				.AsSelf()
				.SingleInstance();
		    builder.Register(c => (ISharingLifetimeScope)c.Resolve<ILifetimeScope>())
			    .As<ISharingLifetimeScope>()
			    .ExternallyOwned();
	    }

	    #region Static

	    public static void Setup(IAppBuilder app)
	    {
		    var apiConfig = new WebApiConfig
		    {
			    LoadAssemblies = true,
			    ConfigureOwinProvider = true
		    };

		    apiConfig.Configure(app);
	    }

		#endregion

    }
}