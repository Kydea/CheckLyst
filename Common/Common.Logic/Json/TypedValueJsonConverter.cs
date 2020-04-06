using System;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Json
{
    public class TypedValueJsonConverter : CustomJsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) => typeof(TypedValue).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobject = JObject.Load(reader);

            var dataType = DataType.FromName(jobject["Type"].Value<string>("TypeName"));
            var value = dataType.FromJson(jobject["Value"], serializer);

            return new TypedValue(dataType, value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Write not implemented");
        }
    }
}