using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public static class PDFExtension
    {
        public static void RenderMigraDocument(this PdfDocument document, Document migraDocument)
        {
            var pdfRenderer = new DocumentRenderer(migraDocument);
            pdfRenderer.PrepareDocument();

            int pages = pdfRenderer.FormattedDocument.PageCount;

            for (int i = 1; i <= pages; i++)
            {
                PageInfo pageInfo = pdfRenderer.FormattedDocument.GetPageInfo(i);

                var page = document.AddPage();
                page.Width = pageInfo.Width;
                page.Height = pageInfo.Height;
                page.Orientation = pageInfo.Orientation;

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    pdfRenderer.RenderPage(gfx, i);
            }
        }

        public static Table CreateTable()
        {
            Table table = new Table();
            table.Rows.LeftIndent = 0;
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Borders.Color = Colors.Black;
            table.Rows.Alignment = RowAlignment.Center;

            return table;
        }

        public static void Add(this Section section, PdfPTable Table)
        {
            section.Add(Table.Render(section));
        }

        // Paragraph
        public static Paragraph Padding(this Paragraph paragraph, Unit size)
        {
            paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = size;
            return paragraph;
        }

        public static Paragraph SetFont(this Paragraph paragraph, Font font)
        {
            paragraph.Format.Font = font.Clone();
            return paragraph;
        }

        public static Paragraph SetText(this Paragraph paragraph, string text, Font font = null)
        {
            if (text != null)
                paragraph.AddText(text);

            if (font == null)
                paragraph.SetFont(PDFSection.GenericFont);

            return paragraph;
        }

        public static Paragraph SetAlignment(this Paragraph paragraph, ParagraphAlignment alignment)
        {
            paragraph.Format.Alignment = alignment;
            return paragraph;
        }

        // WORK IN PROGRESS: THIS WORKS WITH THE 1.50 VERSION (BETA).
        public static Image AddImage(this Paragraph paragraph, byte[] image)
        {
            Image img = paragraph.AddImage($"base64:{Convert.ToBase64String(image)}");
            return img;
        }

        // WORK IN PROGRESS: THIS WORKS WITH THE 1.50 VERSION (BETA).
        // ALTERNATIVE (1): USE LOCAL IMAGE
        // ALTERNATIVE (2): DOWNLOAD IMAGE, SAVE AS TEMP FILE, DELETE IMAGE
        public static Image AddImage(this Paragraph paragraph, Uri uri)
        {
            Image img;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);

            using (WebResponse response = request.GetResponse())
            {
                using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                {
                    const int BUFFER_SIZE = 4096;

                    using (var ms = new MemoryStream())
                    {
                        int count;
                        byte[] buffer = new byte[BUFFER_SIZE];

                        while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                            ms.Write(buffer, 0, count);

                        img = paragraph.AddImage(ms.ToArray());
                    }
                }
            }

            return img;
        }
    }
}
