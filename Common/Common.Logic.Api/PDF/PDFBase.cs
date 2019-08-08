using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class PDFBase
    {

        public static readonly Color Red = Colors.Red;
        public static readonly Color LightGray = Color.FromRgb(235, 235, 235);
        public static readonly Color Gray = Color.FromRgb(204, 204, 204);
        public static readonly Color DarkGray = Color.FromRgb(153, 153, 153);
        public static readonly Color Green = Color.FromRgb(0, 135, 122);
        public static readonly Color Yellow = Color.FromRgb(255, 186, 0);

        public static Border DefaultBorder => new Border { Color = Gray, Style = BorderStyle.Single };
        public static Border RedBorder => new Border { Color = Red, Style = BorderStyle.Single };
       
        public static Font TitleFont1 => FontHelper.GetInstance("Helvetica", 20, FontStyle.Bold, Yellow).Clone();
        public static Font TitleFont2 => FontHelper.GetInstance("Helvetica", 16, FontStyle.Bold, Green).Clone();
        public static Font TitleFont3 => FontHelper.GetInstance("Helvetica", 12, FontStyle.Bold, Green).Clone();
        public static Font GenericFont => FontHelper.GetInstance("Helvetica", 10, FontStyle.Regular, Colors.Black).Clone();
        public static Font BoldFont => FontHelper.GetInstance("Helvetica", 10, FontStyle.Bold, DarkGray).Clone();
        public static Font GrayFont => FontHelper.GetInstance("Helvetica", 10, FontStyle.Regular, DarkGray).Clone();
        public static Font NoteFont => FontHelper.GetInstance("Helvetica", 7, FontStyle.Regular, Colors.Black).Clone();
        public static Font SmallFont => FontHelper.GetInstance("Helvetica", 8, FontStyle.Regular, Colors.Black).Clone();
        public static Font SmallBoldFont => FontHelper.GetInstance("Helvetica", 8, FontStyle.Bold, Colors.Black).Clone();

        protected string Subject { get; private set; }
        protected Document MigraDocDocument { get; private set; }
        protected PdfDocument PdfDocument { get; private set; }
        protected int RenderedPages { get; set; }
        public Section Container { get; private set; }

        public PDFBase(string Subject)
        {
            this.Subject = Subject;
        }

        public async Task AddSectionAsync()
        {
            Container = MigraDocDocument.AddSection();
            await OnNewSectionAsync();
        }

        internal void RenderMigraDocument()
        {
            var pdfRenderer = new DocumentRenderer(MigraDocDocument);
            pdfRenderer.PrepareDocument();

            int pages = pdfRenderer.FormattedDocument.PageCount;
            var startIndex = 1 + RenderedPages;

            for (int i = startIndex; i <= pages; i++)
            {
                PageInfo pageInfo = pdfRenderer.FormattedDocument.GetPageInfo(i);

                var page = PdfDocument.AddPage();
                page.Width = pageInfo.Width;
                page.Height = pageInfo.Height;
                page.Orientation = pageInfo.Orientation;

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    pdfRenderer.RenderPage(gfx, i);

                RenderedPages++;
            }
        }

        public async Task GenerateAsync(Stream Stream)
        {
            try
            {
                PdfDocument = new PdfDocument();
                PdfDocument.Info.Subject = Subject;
                PdfDocument.Info.Keywords = "";
                PdfDocument.Info.Creator = "www.matematici.com";
                PdfDocument.Info.Author = "Matematici";

                MigraDocDocument = new Document();
                
                await AddSectionAsync();
                await OnGenerateAsync();
                
                // Render MigraDoc document into PDFsharp document
                RenderMigraDocument();

                await OnRenderAttachmentsAsync();

                PdfDocument.Save(Stream, false);
                PdfDocument.Close();
                Stream.Flush();
            }
            catch
            {
                throw;
            }
        }

        protected abstract Task OnGenerateAsync();

        protected abstract Task OnRenderAttachmentsAsync();

        protected abstract Task OnNewSectionAsync();
    }
}
