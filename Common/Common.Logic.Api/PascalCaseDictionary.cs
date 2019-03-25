using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CheckListManager.Common.Logic.Api
{
	[JsonDictionary(NamingStrategyType = typeof(DefaultNamingStrategy))]
	public class PascalCaseDictionary<TKey, TValue> : Dictionary<TKey, TValue>
	{
		public PascalCaseDictionary()
		{
		}

		public PascalCaseDictionary(int capacity) : base(capacity)
		{
		}

		public PascalCaseDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
		{
		}

		public PascalCaseDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
		{
		}

		public PascalCaseDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
		{
		}

		public PascalCaseDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
		{
		}

		protected PascalCaseDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
