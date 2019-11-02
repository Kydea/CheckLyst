using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Api.Json
{
    public class CustomJsonFormatter : JsonMediaTypeFormatter
    {
        public HttpRequestMessage Request { get; }

        public CustomJsonFormatter(JsonMediaTypeFormatter formatter, HttpRequestMessage request) : base(formatter)
        {
            Request = request;
        }

        public override JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding)
        {
            //base class does validation
            base.CreateJsonWriter(type, writeStream, effectiveEncoding);

            JsonWriter jsonWriter = new CustomJsonTextWriter(new StreamWriter(writeStream, effectiveEncoding), Request);
            if (Indent)
                jsonWriter.Formatting = Formatting.Indented;

            return jsonWriter;
        }

        public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            //base class does validation
            base.CreateJsonReader(type, readStream, effectiveEncoding);

            return new CustomJsonTextReader(new StreamReader(readStream, effectiveEncoding), Request);
        }
    }
}