using System;
using System.Linq;
using System.Net.Http;
using Autofac;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Api.Json
{
    public class DateTimeOffsetJsonConverter : AutofacJsonConverter<DateTimeOffset?>
    {
        public DateTimeOffsetJsonConverter(ILifetimeScope scope) : base(scope)
        {
        }
        
        private static readonly Type DateTimeOffsetType = typeof(DateTimeOffset?);
        
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
            => DateTimeOffsetType.IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
            {
                var timeZone = ((CustomJsonTextWriter)writer).Request.Headers.TryGetValues("CompanyTimeZone", out var CompanyTimeZones) ? CompanyTimeZones.Single() : null;
                JToken.FromObject(((DateTimeOffset)value).ToLocalTime(timeZone)).WriteTo(writer);
            }
        }

        public override bool CanRead  => true;
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var customReader = (reader as CustomJsonTextReader);
            var timeZone = customReader == null ? TimeZoneInfo.Utc.Id : customReader.Request.Headers.TryGetValues("CompanyTimeZone", out var CompanyTimeZones) ? CompanyTimeZones.Single() : null;
            //var timeZone = ((CustomJsonTextReader)reader).Request.Headers.TryGetValues("CompanyTimeZone", out var CompanyTimeZones) ? CompanyTimeZones.Single() : null;
            //should replace the previous two lines once fixed the cast exception for prototypes with signatures

            if (reader.Value == null)
                return null;
            else if (reader.Value is DateTime dateTimeValue)
            {
                if (dateTimeValue.Kind == DateTimeKind.Local)
                    return new DateTimeOffset(dateTimeValue).ToLocalTime(timeZone);
                else
                    return new DateTimeOffset(dateTimeValue, TimeSpan.Zero).ToLocalTime(timeZone);
            }
            else if (reader.Value is string stringValue)
                return DateTimeOffsetHelper.Parse(stringValue, timeZone, out var Ambiguous, out var TimeSpans);
            else
                throw new ArgumentException("Invalid serialized DateTimeOffset type");
        }

        protected override DateTimeOffset? Create(Type objectType, JObject jsonObject)
        {
            throw new NotImplementedException();
        }
    }
}