using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Api.Json
{
    public class CustomJsonTextReader : JsonTextReader
    {
        public HttpRequestMessage Request { get; }

        public CustomJsonTextReader(TextReader textReader, HttpRequestMessage request) : base(textReader)
        {
            Request = request;
        }
    }
}