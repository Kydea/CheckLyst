using System.Linq;

namespace CheckListManager.Common.Core.Helpers
{
	public static class ObjectHelper
	{
		public static bool In<T>(this T o, params T[] list)
		{
			return list != null && o != null && list.Contains(o);
		}
	}
}
