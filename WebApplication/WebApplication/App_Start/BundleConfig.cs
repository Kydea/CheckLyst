using System.Web;
using System.Web.Optimization;

namespace WebApplication
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection Bundles)
		{
            Bundles.Add(new ScriptBundle("~/Bundles/Libraries").Include("~/Scripts/Libraries/*.js"));
            Bundles.Add(new ScriptBundle("~/Bundles/Matematici")
                .Include("~/Scripts/Matematici/*.js")
                .Include("~/Scripts/Custom/*.js")
                .Include("~/Scripts/Instances/*.js")
                .Include("~/Scripts/Prototypes/*.js")
            );
            Bundles.Add(new StyleBundle("~/Content/Theme/Styles").Include("~/Content/Theme/*.css"));
        }
    }
}
