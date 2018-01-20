using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Template : PdfPageEventHelper
    {
        private CheckListPDF PDF;
        private PdfTemplate headerTemplate, footerTemplate;

        public string DocumentDescription { get; }

        public Template(CheckListPDF PDF, string DocumentDescription)
        {
            this.PDF = PDF;
            this.DocumentDescription = DocumentDescription;
            headerTemplate = PDF.PdfContentByte.CreateTemplate(40, 40);
            footerTemplate = PDF.PdfContentByte.CreateTemplate(40, 40);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            Phrase Description = new Phrase(DocumentDescription, PDFSection.GenericFont);

            //Create Tablele object
            PdfPTable Table = new PdfPTable(1);
            PdfPCell Cell = new PdfPCell(writer.PageNumber != 1 ? Description : null);
            String text = "Page " + writer.PageNumber + " of ";

            PDF.PdfContentByte.BeginText();
            PDF.PdfContentByte.SetFontAndSize(PDFSection.BaseFont, 12);
            PDF.PdfContentByte.SetTextMatrix(document.PageSize.GetRight(105), document.PageSize.GetBottom(55));
            PDF.PdfContentByte.ShowText(text);
            PDF.PdfContentByte.EndText();

            PDF.PdfContentByte.BeginText();
            PDF.PdfContentByte.SetFontAndSize(PDFSection.BaseFont, 12);
            PDF.PdfContentByte.SetTextMatrix(document.PageSize.GetLeft(45), document.PageSize.GetBottom(55));
            PDF.PdfContentByte.ShowText(DateTimeOffset.UtcNow.ToLocalTime(PDF.TimeZone).ToString("dd/MM/yyyy HH:mm"));
            PDF.PdfContentByte.EndText();

            float len = PDFSection.BaseFont.GetWidthPoint(text, 12);
            PDF.PdfContentByte.AddTemplate(footerTemplate, document.PageSize.GetRight(105) + len, document.PageSize.GetBottom(55));

            Cell.HorizontalAlignment = Element.ALIGN_LEFT;

            Cell.Border = 0;

            Table.AddCell(Cell);

            Table.TotalWidth = document.PageSize.Width - 40f;
            Table.WidthPercentage = 100;
            Table.WriteSelectedRows(0, -1, 40, document.PageSize.Height - 60, writer.DirectContent);

            if (writer.PageNumber != 1)
            {
                PDF.PdfContentByte.MoveTo(40, document.PageSize.Height - 100);
                PDF.PdfContentByte.LineTo(document.PageSize.Width - 40, document.PageSize.Height - 100);
                PDF.PdfContentByte.Stroke();
            }

            PDF.PdfContentByte.MoveTo(40, document.PageSize.GetBottom(90));
            PDF.PdfContentByte.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(90));
            PDF.PdfContentByte.Stroke();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(PDFSection.BaseFont, 12);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber).ToString());
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(PDFSection.BaseFont, 12);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber).ToString());
            footerTemplate.EndText();
        }
    }
}
