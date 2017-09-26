using System.Web.Http;
using AutoMapper;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Configuration;

namespace CheckListManager.AccessControl.Api
{
    public class WebApiConfig : ApiConfig
    {
	    protected override void OnCreateMapper(IMapperConfigurationExpression mapperConfig)
	    {
		    base.OnCreateMapper(mapperConfig);

		    mapperConfig.CreateMap<AddUserRequest, Users>()
			    .ForMember(m => m.Password, opt => opt.Ignore());
	    }

		#region Static

	    public static void Register(HttpConfiguration config)
	    {
		    var apiConfig = new WebApiConfig
		    {
			    LoadAssemblies = true
		    };

		    config.Configure(apiConfig);
	    }

		#endregion
	}
}