using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace CheckListManager.WebApplication
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
			filters.Add(new SetCultureFilterAttribute());
        }

		public class SetCultureFilterAttribute : FilterAttribute, IAuthorizationFilter
		{
			public SetCultureFilterAttribute()
			{
			}

			public void OnAuthorization(AuthorizationContext filterContext)
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);
				Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
				Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
				Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator = ",";
			}
		}
    }
}