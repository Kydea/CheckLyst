using System.Web.Http;
using Autofac;
using CheckListManager.Common.Logic.Configuration;

namespace CheckListManager.AccessControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
	        var builder = CreateBuilder();
	        config.Configure(builder);
        }

		public static ContainerBuilder CreateBuilder(bool loadAssemblies = true)
	    {
		    return ApiConfig.CreateBuilder(out var _, loadAssemblies);
	    }
	}
}