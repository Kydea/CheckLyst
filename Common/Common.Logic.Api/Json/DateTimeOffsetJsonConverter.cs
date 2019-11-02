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
            var timeZone = ((CustomJsonTextReader)reader).Request.Headers.TryGetValues("CompanyTimeZone", out var CompanyTimeZones) ? CompanyTimeZones.Single() : null;
            return DateTimeOffsetHelper.Parse(reader.Value as string, timeZone, out var Ambiguous, out var TimeSpans);
        }

        protected override DateTimeOffset? Create(Type objectType, JObject jsonObject)
        {
            throw new NotImplementedException();
        }
    }
}