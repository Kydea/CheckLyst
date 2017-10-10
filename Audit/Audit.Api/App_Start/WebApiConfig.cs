using System.Web.Http;
using CheckListManager.Common.Logic.Api.Configuration;

namespace CheckListManager.Audit.Api
{
    public class WebApiConfig : ApiConfig
    {

		#region Static

	    public static void Register(HttpConfiguration config)
	    {
		    config.Configure(new WebApiConfig());
	    }

		#endregion
	}
}