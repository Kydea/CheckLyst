using System.Web.Http;
using CheckListManager.Common.Logic.Api.Configuration;

namespace CheckListManager.Instances.Api
{
    public class WebApiConfig : ApiConfig
    {
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