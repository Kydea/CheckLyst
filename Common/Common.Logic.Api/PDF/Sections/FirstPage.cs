using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

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
            var CheckListPDF = (CheckListPDF)PDFBase;
            Image Logo = Image.GetInstance(new Uri($"{CheckListPDF.Repository}PDF/Logo-{CheckListPDF.CompanyID}.png"));
            Logo.ScaleToFit(PDFBase.Document.PageSize.Width / 3, 400f);
            Logo.Alignment = Element.ALIGN_CENTER;
            PDFBase.Document.Add(Logo);

            Paragraph Text = new Paragraph(new Phrase("\n" + FirstRow + "\n"));
            Text.Alignment = Element.ALIGN_CENTER;
            PDFBase.Document.Add(Text);

            Text = new Paragraph(new Phrase(SecondRow + "\n"));
            Text.Alignment = Element.ALIGN_CENTER;
            PDFBase.Document.Add(Text);

            Text = new Paragraph(new Phrase(ThirdRow + "\n\n\n"));
            Text.Alignment = Element.ALIGN_CENTER;
            PDFBase.Document.Add(Text);

            PdfPTable Table = new PdfPTable(2);

            Table.HorizontalAlignment = Element.ALIGN_CENTER;

            foreach (var Row in Rows)
            {
                Table.AddCell(new PdfPCell(new Paragraph(Row.Item1, PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(Row.Item2, PDFSection.GenericFont)) { Padding = 10 });
            }

            PDFBase.Document.Add(Table);
            PDFBase.NewPage();
        }
    }
}
