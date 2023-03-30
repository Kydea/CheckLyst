using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.PDF.Helper;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using System.Runtime.Remoting.Messaging;
using CheckListManager.Common.Core.Helpers;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportPDF : PDFBase
    {
        public long CompanyID { get; }
        public string CompanyName { get; }
        public string TimeZone { get; }
        public string Repository { get; }
        public ReportResponse Report { get; }

        public IConfigurationService ConfigurationService { get; }

        public ReportPDF(long CompanyID, string CompanyName, string TimeZone, string Repository, IConfigurationService ConfigurationService, ReportResponse Report) : base("CheckList")
        {
            this.CompanyID = CompanyID;
            this.CompanyName = CompanyName;
            this.TimeZone = TimeZone;
            this.Repository = Repository;
            this.ConfigurationService = ConfigurationService;
            this.Report = Report;
        }

        protected override async Task OnGenerateAsync()
        {
            Container.Headers = null;
            Container.PageSetup.TopMargin = 36;

            Paragraph paragraph = Container.AddParagraph();

            paragraph.Format.Alignment = ParagraphAlignment.Center;
            Image logo = paragraph.AddImage(new Uri($"{Repository}PDF/Logo-{CompanyID}.png"));

            if (logo != null)
                logo.ScaleWidth = 0.4;

            paragraph = Container.AddParagraph("prima riga");
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceBefore = 20;
            paragraph.Format.SpaceAfter = 10;

            paragraph = Container.AddParagraph();
            paragraph.Format.Borders.Bottom = DefaultBorder;
            paragraph = Container.AddParagraph("seconda riga");
            paragraph.Format.SpaceBefore = paragraph.Format.SpaceAfter = 20;

            int Columns = 2;
            PdfPTable Table = new PdfPTable(Columns) { WidthPercentage = 100, HorizontalAlignment = RowAlignment.Center };
            Table.SetWidths(new float[] { 23.66f, 100 - 23.66f });

            var Rows = new List<Tuple<string, string>> {
                new Tuple<string, string> ("Plant", CompanyName),
                new Tuple<string, string> ("Report", "..."),
                new Tuple<string, string> ("State", "...")
            };

            foreach (var Row in Rows)
            {
                Table.AddCell(new PdfPCell(new Paragraph().SetText(Row.Item1).SetFont(PDFBase.BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText(Row.Item2).Padding(5)));
                Table.CompleteRow();
            }
            Container.Add(Table);

            paragraph = Container.AddParagraph("terza riga");
            paragraph.Format.SpaceBefore = 20;

            await Task.CompletedTask;
        }

        protected override Task OnRenderAttachmentsAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task OnNewSectionAsync()
        {
            PageSetup setup = Container.Document.DefaultPageSetup.Clone();
            setup.PageFormat = PageFormat.A4;
            setup.LeftMargin = setup.RightMargin = Unit.FromPoint(40);
            setup.TopMargin = Unit.FromPoint(70);
            setup.BottomMargin = Unit.FromPoint(80);
            setup.FooterDistance = Unit.FromPoint(61);
            setup.HeaderDistance = Unit.FromPoint(0);
            //setup.DifferentFirstPageHeaderFooter = true;

            Container.PageSetup = setup;

            Style style = Container.Document.Styles[StyleNames.Normal];
            style.Font = GenericFont.Clone();

            Paragraph header;
            header = Container.Headers.Primary.AddParagraph("Header");
            header.Format.SpaceBefore = 20;
            header.Format.Alignment = ParagraphAlignment.Right;
            header.Format.Borders.Top = DefaultBorder;
            header.Format.Borders.Bottom = DefaultBorder;
            header.Format.Borders.DistanceFromTop = 10;
            header.Format.Borders.DistanceFromBottom = 10;
            header.Format.Font = GrayFont;

            Paragraph footer = Container.Footers.Primary.AddParagraph();
            footer.Tag = "PageCount";
            footer.Format.Font = GrayFont;
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" / ");
            footer.AddNumPagesField();
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.Borders.Top = DefaultBorder;
            footer.Format.Borders.DistanceFromTop = 10;
            footer.Format.SpaceAfter = new Unit(-footer.Format.Font.Size, UnitType.Point);

            footer = Container.Footers.Primary.AddParagraph();
            footer.Format.Font = GrayFont;
            footer.AddText($"Generated on {DateTimeOffset.UtcNow.ToLocalTime(TimeZone):yyyy-MM-dd HH:mm UTC zzz} - RESTRICTED");
            footer.Format.SpaceAfter = new Unit(-footer.Format.Font.Size, UnitType.Point);

            return Task.CompletedTask;
        }
    }
}