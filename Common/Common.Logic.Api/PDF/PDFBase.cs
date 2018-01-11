using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class PDFBase
    {
        protected String Subject;

        internal Document Document { get; private set; }
        internal PdfContentByte PdfContentByte { get; private set; }
        internal PdfWriter PdfWriter { get; private set; }

        public float CurrentRenderingY = 0, CurrentRenderingX = 0, PageStartY = 0;

        public PDFBase(String Subject)
        {
            this.Subject = Subject;
        }

        protected virtual Document CreateDocument()
        {
            return new Document();
        }

        public void Generate(Stream Stream)
        {
            try
            {
                Document = CreateDocument();
                PdfWriter = PdfWriter.GetInstance(Document, Stream);
                Document.Open();

                PdfContentByte = PdfWriter.DirectContent;

                Document.AddSubject(Subject);
                Document.AddKeywords("");
                Document.AddCreator("www.matematici.com");
                Document.AddAuthor("Matematici");

                OnGenerate();

                Stream.Flush();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (Document != null && Document.IsOpen())
                    try { Document.Close(); }
                    catch { }
            }
        }

        abstract protected void OnGenerate();

        internal void NewPage()
        {
            Document.NewPage();
        }

        internal static Image GetImage(System.Drawing.Image Source, int? Alignment, int? Scale)
        {
            return GetImage(Image.GetInstance(Source, System.Drawing.Imaging.ImageFormat.Png), Alignment, Scale);
        }

        internal static Image GetImage(byte[] Bytes, int? Alignment, int? Scale)
        {
            return GetImage(Image.GetInstance(Bytes), Alignment, Scale);
        }

        internal static Image GetImage(string Path, int? Alignment, int? Scale)
        {
            return GetImage(Image.GetInstance(HttpContext.Current.Server.MapPath(Path)), Alignment, Scale);
        }

        internal static Image GetImage(Image Image, int? Alignment, int? Scale)
        {
            if (Alignment.HasValue)
                Image.Alignment = Alignment.Value;
            if (Scale.HasValue)
                Image.ScalePercent(Scale.Value);

            return Image;
        }
    }
}