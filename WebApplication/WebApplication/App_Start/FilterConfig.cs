using System.Web.Mvc;

namespace CheckListManager.WebApplication
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
        }
    }
}
