using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Matematici
{
	public class DynamicDictionary<T> : DynamicObject
	{
		private readonly Dictionary<String, T> Dictionary;

		public DynamicDictionary(Dictionary<String, T> Dictionary)
		{
			this.Dictionary = Dictionary;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return Dictionary.Keys;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (Dictionary.ContainsKey(binder.Name))
			{
				result = Dictionary[binder.Name];
				return true;
			}

			result = null;
			return false;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (value is T)
			{
				Dictionary[binder.Name] = (T)value;
				return true;
			}

			return false;
		}
	}
}