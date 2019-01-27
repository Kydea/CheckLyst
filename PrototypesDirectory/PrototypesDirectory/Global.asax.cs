using Newtonsoft.Json;
using System.IO;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Matematici
{
	public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ModelBinders.Binders.DefaultBinder = new Binders.ModelBinder();

            using (StreamReader StreamReader = new StreamReader(Server.MapPath("~/Configuration.json")))
                Settings.Provider.Values = JsonConvert.DeserializeObject<Settings.Provider>(StreamReader.ReadToEnd());

            Settings.Provider.Values.Setup();
            DataSource.Engine.Initialize();
        }

		protected void Application_End()
		{
		}
    }
}