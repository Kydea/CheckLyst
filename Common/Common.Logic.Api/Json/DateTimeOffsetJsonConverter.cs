using System;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Api.Json
{
    public class DateTimeOffsetJsonConverter : JsonConverter
    {
        private static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);
        
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
            => DateTimeOffsetType.IsAssignableFrom(objectType);


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string timeZone = null;
            if (writer is CustomJsonTextWriter jsonWriter)
                timeZone = jsonWriter.Request.Headers
                    .TryGetValues("CompanyTimeZone", out var CompanyTimeZones) ? CompanyTimeZones.Single() : null;
            
            if (value is DateTimeOffset dateTimeOffset)
                JToken.FromObject(dateTimeOffset.ToLocalTime(timeZone)).WriteTo(writer);
            else
                writer.WriteNull();
        }

        public override bool CanRead  => false;
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}