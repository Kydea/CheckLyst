using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Api.Json
{
    public class CustomJsonTextWriter : JsonTextWriter
    {
        public HttpRequestMessage Request { get; }

        public CustomJsonTextWriter(TextWriter textWriter, HttpRequestMessage request) : base(textWriter)
        {
            Request = request;
        }
    }
}