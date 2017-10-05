using System.Web.Optimization;

namespace CheckListManager.WebApplication
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
            bundles.Add(new ScriptBundle("~/Bundles/Libraries").Include("~/Scripts/Libraries/*.js"));
            bundles.Add(new ScriptBundle("~/Bundles/Matematici")
                .Include("~/Scripts/Matematici/*.js")
                .Include("~/Scripts/Controls/*.js")
                .Include("~/Scripts/Custom/*.js")
                .Include("~/Scripts/Instances/*.js")
                .Include("~/Scripts/Prototypes/*.js")
            );
            bundles.Add(new StyleBundle("~/Content/Theme/Styles").Include("~/Content/Theme/*.css"));
        }
    }
}
