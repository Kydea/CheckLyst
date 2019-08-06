using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class FirstPage : PDFSection
    {
        public FirstPage(CheckListPDF PDFBase, string FirstRow, string SecondRow, string ThirdRow, List<Tuple<string, string>> Rows) : base(PDFBase, 0)
        {
            this.FirstRow = FirstRow;
            this.SecondRow = SecondRow;
            this.ThirdRow = ThirdRow;
            this.Rows = Rows;
        }

        private string FirstRow { get; }
        private string SecondRow { get; }
        private string ThirdRow { get; }
        private List<Tuple<string, string>> Rows { get; }

        public override Task RenderAsync(params object[] State)
        {
            PDFBase.Container.Headers = null;
            PDFBase.Container.PageSetup.TopMargin = 36;

            Paragraph paragraph = PDFBase.Container.AddParagraph();

            paragraph.Format.Alignment = ParagraphAlignment.Center;
            var CheckListPDF = (CheckListPDF)PDFBase;
            Image logo = paragraph.AddImage(new Uri($"{CheckListPDF.Repository}PDF/Logo-{CheckListPDF.CompanyID}.png"));

            if (logo != null)
                logo.ScaleWidth = 0.4;

            if (!string.IsNullOrWhiteSpace(FirstRow))
            {
                paragraph = PDFBase.Container.AddParagraph(FirstRow);
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.SpaceBefore = 20;
                paragraph.Format.SpaceAfter = 10;
                paragraph = PDFBase.Container.AddParagraph();
                paragraph.Format.Borders.Bottom = PDFBase.DefaultBorder;
            }

            if (!string.IsNullOrWhiteSpace(SecondRow))
            {
                paragraph = PDFBase.Container.AddParagraph(SecondRow);
                paragraph.Format.SpaceBefore = paragraph.Format.SpaceAfter = 20;
            }

            PdfPTable Table = new PdfPTable(2) { WidthPercentage = 100, HorizontalAlignment = RowAlignment.Center };
            foreach (var Row in Rows)
            {
                Table.AddCell(new PdfPCell(new Paragraph().SetText(Row.Item1).SetFont(PDFBase.BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText(Row.Item2).Padding(5)));
                Table.CompleteRow();
            }
            PDFBase.Container.Add(Table);

            if (!string.IsNullOrWhiteSpace(ThirdRow))
            {
                paragraph = PDFBase.Container.AddParagraph(ThirdRow);
                paragraph.Format.SpaceBefore = 20;
            }

            return Task.CompletedTask;
        }
    }
}
