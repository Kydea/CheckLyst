using System.IO;

namespace CheckListManager.Common.Logic.Models
{
    public class StorageFile
    {
        public Stream Content { get; }
        public string ContentType { get; }

        public StorageFile(Stream content, string contentType)
        {
            Content = content;
            ContentType = contentType;
        }
    }
}