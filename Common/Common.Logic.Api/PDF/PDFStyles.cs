using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using System;
using System.Net;
using System.IO;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public static class PDFExtension
    {
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

        public static void RemovePageCount(this Section section)
        {
            for (int index = 0; index < section.Footers.Primary.Elements.Count; index++)
            {
                DocumentObject Element = section.Footers.Primary.Elements[index];

                if(Element.Tag is string Tag && Tag == "PageCount" && Element is Paragraph paragraph)
                {
                    paragraph.Elements.Clear();
                    break;
                }
            }
        }

        public static void Remove(this Section Section)
        {
            var Document = Section.Document;
            Document.Sections.RemoveObjectAt(Document.Sections.IndexOf(Section));
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
                paragraph.SetFont(PDFBase.GenericFont);

            return paragraph;
        }

        public static Paragraph SetAlignment(this Paragraph paragraph, ParagraphAlignment alignment)
        {
            paragraph.Format.Alignment = alignment;
            return paragraph;
        }

        public static Image AddImage(this Paragraph paragraph, byte[] image)
        {
            Image img = paragraph.AddImage($"base64:{Convert.ToBase64String(image)}");
            return img;
        }

        public static Image AddImage(this Paragraph paragraph, Uri uri)
        {
            Image img = null;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);

            try
            {
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
            }
            catch (WebException e)
            {
                HttpWebResponse response = e.Response as HttpWebResponse;

                if (response?.StatusCode == HttpStatusCode.NotFound)
                    paragraph.AddText(String.Format("Image \"{0}\" not found.", uri));
                else
                    paragraph.AddText($"Error: {e.Message}");
            }

            return img;
        }
    }
}
