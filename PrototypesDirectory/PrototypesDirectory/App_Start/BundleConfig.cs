using System.Web;
using System.Web.Optimization;

namespace CheckListManager.WebApplication
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection Bundles)
        {
            Bundles.Add(new ScriptBundle("~/Bundles/Libraries").Include("~/Scripts/Libraries/*.js"));
            Bundles.Add(new ScriptBundle("~/Bundles/Matematici")
                .Include("~/Scripts/Matematici/*.js")
                .Include("~/Scripts/Controls/*.js")
                .Include("~/Scripts/Custom/*.js")
            );
            Bundles.Add(new StyleBundle("~/Content/Theme/Styles").Include("~/Content/Theme/*.css"));
            Bundles.IgnoreList.Ignore("*.min.css", OptimizationMode.Always);
        }
    }
}