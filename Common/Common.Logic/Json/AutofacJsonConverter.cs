using System;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Json
{
	public abstract class AutofacJsonConverter<TBase> : CustomJsonConverter
	{
		protected ILifetimeScope Scope { get; }

		protected AutofacJsonConverter(ILifetimeScope scope)
		{
			Scope = scope;
		}

		protected abstract TBase Create(Type objectType, JObject jsonObject);

		public override bool CanConvert(Type objectType) => typeof(TBase).IsAssignableFrom(objectType);

		public override bool CanWrite => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jsonObject = JObject.Load(reader);
			var target = Create(objectType, jsonObject);

			serializer.Populate(jsonObject.CreateReader(), target);

			return target;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
