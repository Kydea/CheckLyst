using Autofac;
using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Json
{
    public class DateTimeOffsetJsonConverter : AutofacJsonConverter<DateTimeOffset>
    {
        public DateTimeOffsetJsonConverter(ILifetimeScope scope) : base(scope)
        {
        }

        protected override DateTimeOffset Create(Type objectType, JObject jsonObject) =>
            throw new NotImplementedException();

        public override bool CanWrite => true;

        public override bool CanRead  => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //var request = Scope.Resolve<HttpRequestMessage>();
            var dateTimeOffset = value as DateTimeOffset?;
            if (dateTimeOffset.HasValue)
                serializer.Serialize(writer, dateTimeOffset.Value.ToLocalTime("SA Western Standard Time"));
            else
                writer.WriteNull();
        }
    }
}
