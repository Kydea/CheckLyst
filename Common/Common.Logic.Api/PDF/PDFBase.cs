using MigraDoc.DocumentObjectModel;
using PdfSharp.Pdf;
using System.IO;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class PDFBase
    {
        protected string Subject;
        public Section Container;

        public PDFBase(string Subject)
        {
            this.Subject = Subject;
        }

        public void Generate(Stream Stream)
        {
            try
            {
                PdfDocument Document = new PdfDocument();
                Document.Info.Subject = Subject;
                Document.Info.Keywords = "";
                Document.Info.Creator = "www.matematici.com";
                Document.Info.Author = "Matematici";

                Container = new MigraDoc.DocumentObjectModel.Document().AddSection();

                OnGenerate();

                // Render MigraDoc document into PDFsharp document
                Document.RenderMigraDocument(Container.Document);

                Document.Save(Stream, false);
                Document.Close();
                Stream.Flush();
            }
            catch
            {
                throw;
            }
        }

        abstract protected void OnGenerate();
    }
}
