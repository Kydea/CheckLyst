using System.Web;
using System.Web.Optimization;

namespace Embedder
{
    public class BundleConfig
    {
        // Per altre informazioni sulla creazione di bundle, vedere https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include("~/scripts/*.js"));
            bundles.Add(new StyleBundle("~/bundles/styles").Include("~/content/*.css"));
        }
    }
}
