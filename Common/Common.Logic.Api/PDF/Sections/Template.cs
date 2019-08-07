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
        public Template(CheckListPDF PDF, string Header)
        {
            PageSetup setup = PDF.Container.Document.DefaultPageSetup.Clone();
            setup.PageFormat = PageFormat.A4;
            setup.LeftMargin = setup.RightMargin = Unit.FromPoint(40);
            setup.TopMargin = Unit.FromPoint(70);
            setup.BottomMargin = Unit.FromPoint(80);
            setup.FooterDistance = Unit.FromPoint(61);
            setup.HeaderDistance = Unit.FromPoint(0);
            //setup.DifferentFirstPageHeaderFooter = true;

            PDF.Container.PageSetup = setup;

            Style style = style = PDF.Container.Document.Styles[StyleNames.Normal];
            style.Font = PDFBase.GenericFont.Clone();

            Paragraph header;
            header = PDF.Container.Headers.Primary.AddParagraph(Header ?? string.Empty);
            header.Format.SpaceBefore = 20;
            header.Format.Alignment = ParagraphAlignment.Right;
            header.Format.Borders.Top = PDFBase.DefaultBorder;
            header.Format.Borders.Bottom = PDFBase.DefaultBorder;
            header.Format.Borders.DistanceFromTop = 10;
            header.Format.Borders.DistanceFromBottom = 10;
            header.Format.Font = PDFBase.GrayFont;

            Paragraph footer = PDF.Container.Footers.Primary.AddParagraph();
            footer.Format.Font = PDFBase.GrayFont;
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" / ");
            footer.AddNumPagesField();
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.Borders.Top = PDFBase.DefaultBorder;
            footer.Format.Borders.DistanceFromTop = 10;
            footer.Format.SpaceAfter = new Unit(-footer.Format.Font.Size, UnitType.Point);

            footer = PDF.Container.Footers.Primary.AddParagraph();
            footer.Format.Font = PDFBase.GrayFont;
            footer.AddText($"Generated on {DateTimeOffset.UtcNow.ToLocalTime(PDF.TimeZone):yyyy-MM-dd HH:mm UTC zzz} - RESTRICTED");
            footer.Format.SpaceAfter = new Unit(-footer.Format.Font.Size, UnitType.Point);
        }

        public void Render(FirstPage firstPage = null)
        {

        }
    }
}
