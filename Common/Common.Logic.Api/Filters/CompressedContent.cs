using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public sealed class CompressedContent : HttpContent
{
    private readonly HttpContent _original;
    private readonly bool _useGZIPCompression; // "gzip" o "deflate"

    public CompressedContent(HttpContent original, bool useGZIPCompression)
    {
        _original = original;
        _useGZIPCompression = useGZIPCompression;

        foreach (var h in _original.Headers)
            Headers.TryAddWithoutValidation(h.Key, h.Value);

        Headers.ContentEncoding.Clear();
        Headers.ContentEncoding.Add(_useGZIPCompression ? "gzip" : "deflate");
        Headers.ContentLength = null;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        using (Stream zip = _useGZIPCompression ? (Stream)new GZipStream(stream, CompressionLevel.Fastest, leaveOpen: true) : new DeflateStream(stream, CompressionLevel.Fastest, leaveOpen: true)) 
            await _original.CopyToAsync(zip);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }
}