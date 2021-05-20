using CheckListManager.Common.Logic.Api.Configuration;
using Owin;

namespace CheckListManager.Reports.Api
{
    public class WebApiConfig : ApiConfig
    {
		#region Static

	    public static void Setup(IAppBuilder app)
	    {
		    var apiConfig = new WebApiConfig
		    {
			    LoadAssemblies = true
		    };

		    apiConfig.Configure(app);
	    }

		#endregion
	}
}