using Newtonsoft.Json;

namespace CheckListManager.Common.Core.Helpers
{
	public static class JsonHelper
	{
		public static object ToObject(this string source, JsonSerializerSettings settings = null)
		{
			return !string.IsNullOrEmpty(source)
				? JsonConvert.DeserializeObject(source, settings ?? new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
				: null;
		}

		public static T ToObject<T>(this string source, JsonSerializerSettings settings = null)
		{
			return !string.IsNullOrEmpty(source)
				? JsonConvert.DeserializeObject<T>(source, settings ?? new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
				: default(T);
		}

		public static string ToJson(this object source, JsonSerializerSettings settings = null)
		{
			return source != null
				? JsonConvert.SerializeObject(source, settings ?? new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
				: null;
		}

		public static string ToJson<T>(this T source, JsonSerializerSettings settings = null)
		{
			return source != null
				? JsonConvert.SerializeObject(source, settings ?? new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
				: null;
		}
	}
}
