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
            setup.LeftMargin = setup.RightMargin = Unit.FromPoint(30);
            setup.TopMargin = Unit.FromPoint(70);
            setup.BottomMargin = Unit.FromPoint(50);
            setup.FooterDistance = Unit.FromPoint(10);
            setup.HeaderDistance = Unit.FromPoint(20);
            setup.DifferentFirstPageHeaderFooter = true;

            PDF.Container.PageSetup = setup;
            // END_PAGE_FORMAT

            // STYLE
            Style style = style = PDF.Container.Document.Styles[StyleNames.Normal];
            style.Font = PDFSection.GenericFont.Clone();
            // END_STYLE

            Paragraph header = PDF.Container.Headers.Primary.AddParagraph(DocumentDescription);
            header.Format.Borders.Bottom.Color = Colors.Black;
            header.Format.Borders.Bottom.Width = 0.5;
            header.Format.Borders.DistanceFromBottom = 5;

            // Page counter
            Paragraph footer = PDF.Container.Footers.Primary.AddParagraph();
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" of ");
            footer.AddNumPagesField();
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.Borders.Top.Color = Colors.Black;
            footer.Format.Borders.Top.Width = 0.5;
            footer.Format.Borders.DistanceFromTop = 5;
            PDF.Container.Footers.FirstPage.Add(footer.Clone());

            // Time
            footer = PDF.Container.Footers.Primary.AddParagraph();
            footer.AddText($"Generated on {DateTimeOffset.UtcNow.ToLocalTime(PDF.TimeZone):yyyy-MM-dd HH:mm UTC zzz} - RESTRICTED");
            footer.Format.Borders.DistanceFromTop = 5;
            footer.Format.SpaceAfter = new Unit(-footer.Format.Font.Size, UnitType.Point);
            PDF.Container.Footers.FirstPage.Add(footer.Clone());
        }
    }
}
