using System;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Json
{
	public abstract class AutofacJsonConverter<TBase> : JsonConverter
	{
		protected ILifetimeScope Scope { get; }

		protected AutofacJsonConverter(ILifetimeScope scope)
		{
			Scope = scope;
		}

		protected abstract TBase Create(Type objectType, JObject jObject);

		public override bool CanConvert(Type objectType) => typeof(TBase).IsAssignableFrom(objectType);

		public override bool CanWrite => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jObject = JObject.Load(reader);
			var target = Create(objectType, jObject);

			serializer.Populate(jObject.CreateReader(), target);

			return target;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
