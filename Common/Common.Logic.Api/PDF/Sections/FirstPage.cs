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
        public FirstPage(CheckListPDF PDFBase, string Prototype, string Instance, string InstanceDescription, List<Tuple<string, string>> Rows) : base(PDFBase, 0)
        {
            this.FirstRow = Prototype;
            this.SecondRow = Instance;
            this.ThirdRow = InstanceDescription;
            this.Rows = Rows;
        }

        private string FirstRow { get; }
        private string SecondRow { get; }
        private string ThirdRow { get; }
        private List<Tuple<string, string>> Rows { get; }

        public override void Render(params Object[] State)
        {
            Paragraph paragraph = PDFBase.Container.AddParagraph();

            paragraph.Format.Alignment = ParagraphAlignment.Center;
            var CheckListPDF = (CheckListPDF)PDFBase;
            Image logo = paragraph.AddImage(new Uri($"{CheckListPDF.Repository}PDF/Logo-{CheckListPDF.CompanyID}.png"));
            logo.ScaleWidth = 0.5;

            paragraph = PDFBase.Container.AddParagraph().SetText("\n" + FirstRow + "\n");
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            paragraph = PDFBase.Container.AddParagraph().SetText(SecondRow + "\n");
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            paragraph = PDFBase.Container.AddParagraph().SetText(ThirdRow + "\n\n\n");
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // Define table
            PdfPTable Table = new PdfPTable(2);

            foreach (var Row in Rows)
            {
                Table.AddCell(new PdfPCell(new Paragraph().SetText(Row.Item1).SetFont(GenericBoldFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText(Row.Item2).Padding(10)));
                Table.CompleteRow();
            }

            PDFBase.Container.Add(Table);
            PDFBase.Container.AddPageBreak();
        }
    }
}
