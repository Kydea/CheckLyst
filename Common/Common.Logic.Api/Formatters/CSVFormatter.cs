using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.Formatters
{
    public class CSVFormatter<T> : MediaTypeFormatter
    {

        //aggiungere costrutturi per:
        //ContentLength
        //FileName
        //Model
        //Handler per righe e intestazione
        public CSVFormatter(string[] Header, string FileName, Func<T, string[]> GetRow)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));
            this.Header = Header;
            this.FileName = string.IsNullOrEmpty(FileName) || FileName.EndsWith(".csv") ? FileName : FileName + ".csv";
            GetValues = GetRow;
        }

        private readonly Func<T, string[]> GetValues;

        private string[] Header { get; }

        private string FileName { get; }

        public string Separator { get; set; } = ";";

        public string Delimitator { get; set; } = "\"";

        public string RowSeparator { get; set; } = "\n";

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(IEnumerable<T>).IsAssignableFrom(type);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            headers.ContentType = new MediaTypeHeaderValue("text/csv");
            headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = FileName };

            base.SetDefaultContentHeaders(type, headers, mediaType);
        }

        private async Task WriteRowAsync (StreamWriter StreamWriter, string[] Values)
        {
            foreach (var Value in Values)
                await StreamWriter.WriteAsync(Escape(Value));
            await StreamWriter.WriteAsync(RowSeparator);
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
        {
            var StreamWriter = new StreamWriter(stream);
            await WriteRowAsync(StreamWriter, Header);
            foreach (var Row in (IEnumerable<T>)value)
                await WriteRowAsync(StreamWriter, GetValues(Row));
            await StreamWriter.FlushAsync();
        }

        private string Escape(string Value)
        {
            return Delimitator + (Value ?? string.Empty).Replace(Delimitator, "\\" + Delimitator) + Delimitator + Separator;
        }
    }
}
