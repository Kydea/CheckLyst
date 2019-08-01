using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Pdf;
using System.IO;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class PDFBase
    {
        public static Font NoteFont => FontHelper.GetInstance("Helvetica", 7, FontStyle.Regular, Colors.Black).Clone();
        public static Font SmallFont => FontHelper.GetInstance("Helvetica", 8, FontStyle.Regular, Colors.Black).Clone();
        public static Font SmallBoldFont => FontHelper.GetInstance("Helvetica", 8, FontStyle.Bold, Colors.Black).Clone();
        public static Font NormalFont => FontHelper.GetInstance("Helvetica", 9, FontStyle.Regular, Colors.Black).Clone();
        public static Font GenericFont => FontHelper.GetInstance("Helvetica", 10, FontStyle.Regular, Colors.Black).Clone();
        public static Font GenericBoldFont => FontHelper.GetInstance("Helvetica", 10, FontStyle.Bold, Colors.DimGray).Clone();
        public static Font GenericFontGray => FontHelper.GetInstance("Helvetica", 10, FontStyle.Regular, Colors.DimGray).Clone();
        public static Font FirstHeaderFont => FontHelper.GetInstance("Helvetica", 14, FontStyle.Bold, Colors.Black).Clone();
        public static Font SecondHeaderFont => FontHelper.GetInstance("Helvetica", 10, FontStyle.Bold, Colors.Black).Clone();

        public static readonly Color BorderColor = Color.FromRgb(204, 204, 204);
        public static Border DefaultBorder => new Border { Color = BorderColor, Style = BorderStyle.Single };


        protected string Subject;

        protected Document PdfDocument;
        public Section Container;

        public PDFBase(string Subject)
        {
            this.Subject = Subject;
        }

        public void AddSection()
        {
            Container = PdfDocument.AddSection();
            OnNewSection();
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

                PdfDocument = new Document();
                AddSection();

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

        abstract protected void OnNewSection();
    }
}
