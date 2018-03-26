using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using MigraDoc.DocumentObjectModel;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Template
    {
        public Template(CheckListPDF PDF, string DocumentDescription)
        {
            // PAGE_FORMAT
            PageSetup setup = PDF.Container.Document.DefaultPageSetup.Clone();
            setup.PageFormat = PageFormat.A4;
            setup.LeftMargin = setup.RightMargin = Unit.FromPoint(40);
            setup.TopMargin = Unit.FromPoint(136);
            setup.BottomMargin = Unit.FromPoint(0);
            setup.FooterDistance = Unit.FromPoint(61);
            setup.HeaderDistance = Unit.FromPoint(0);
            //setup.DifferentFirstPageHeaderFooter = true;

            PDF.Container.PageSetup = setup;

            // STYLE
            Style style = style = PDF.Container.Document.Styles[StyleNames.Normal];
            style.Font = PDFSection.GenericFont.Clone();
            // END_STYLE

            Paragraph header = PDF.Container.Headers.Primary.AddParagraph(DocumentDescription);
            header.Format.Borders.Bottom.Color = Colors.Black;
            header.Format.Borders.Bottom.Width = new Unit(0.75, UnitType.Point);
            header.Format.Borders.DistanceFromTop = 58;
            header.Format.Borders.DistanceFromBottom = 15;
            header.Format.Font.Size = 12;

            // Page counter
            Paragraph footer = PDF.Container.Footers.Primary.AddParagraph();
            footer.Format.Font.Size = 12;
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" of ");
            footer.AddNumPagesField();
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.Borders.Top.Color = Colors.Black;
            footer.Format.Borders.Top.Width = new Unit(0.75, UnitType.Point);
            footer.Format.Borders.DistanceFromTop = 25;
            footer.Format.SpaceAfter = new Unit(-footer.Format.Font.Size, UnitType.Point);

            // Time
            footer = PDF.Container.Footers.Primary.AddParagraph();
            footer.Format.Font.Size = 12;
            footer.AddText($"Generated on {DateTimeOffset.UtcNow.ToLocalTime(PDF.TimeZone):yyyy-MM-dd HH:mm UTC zzz} - RESTRICTED");
            footer.Format.SpaceAfter = new Unit(-footer.Format.Font.Size, UnitType.Point);
        }

        public void Render(FirstPage firstPage = null)
        {

        }
    }
}
