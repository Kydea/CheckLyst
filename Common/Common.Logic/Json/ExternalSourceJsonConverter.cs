using System;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.Expressions;
using CheckListManager.Common.Logic.Models.ExternalSources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Json
{
    public class ExternalSourceJsonConverter : CustomJsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        
        public override bool CanConvert(Type objectType) => typeof(ExternalSource).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string stringValue)
            {
                //in v. < 3.0.0 external sources was a dictionary of strings
                var code = reader.Path.Substring(reader.Path.LastIndexOf('.') + 1);
                return new ExternalSource { Code = code, DataSource = stringValue };
            }
            else
            {
                JObject jobject = JObject.Load(reader);
                var externalSource = jobject.ToObject<ExternalSource>();
                return externalSource;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException("Write not implemented");
    }
}