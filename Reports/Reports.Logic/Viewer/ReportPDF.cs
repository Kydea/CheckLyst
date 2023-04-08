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
using static System.Collections.Specialized.BitVector32;
using MigraDoc.Rendering;
using System.Xml.Linq;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportPDF : PDFBase
    {
        public long CompanyID { get; }
        public string CompanyName { get; }
        public string TimeZone { get; }
        public string Repository { get; }
        PageFormat Format { get; }
        Orientation Orientation { get; }
        public string User { get; }
        public ReportResponse Report { get; }


        public IConfigurationService ConfigurationService { get; }

        public ReportPDF(
            long CompanyID, 
            string CompanyName, 
            string TimeZone, 
            string Repository, 
            IConfigurationService ConfigurationService, 
            PageFormat Format, 
            Orientation Orientation, 
            string User,
            ReportResponse Report
        ) : base("CheckList")
        {
            this.CompanyID = CompanyID;
            this.CompanyName = CompanyName;
            this.TimeZone = TimeZone;
            this.Repository = Repository;
            this.ConfigurationService = ConfigurationService;
            this.User = User;
            this.Report = Report;
            this.Format = Format;
            this.Orientation = Orientation;
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

            var table = Container.AddTable();
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            var pageWidth = Orientation == Orientation.Portrait ? 
                Container.PageSetup.PageWidth - Container.PageSetup.LeftMargin - Container.PageSetup.RightMargin :
                Container.PageSetup.PageHeight - Container.PageSetup.TopMargin - Container.PageSetup.BottomMargin;
            var column = table.AddColumn();
            column.Width = pageWidth * .3;

            column = table.AddColumn();
            column.Width = pageWidth * .7;

            var Rows = new List<Tuple<string, string>> {
                new Tuple<string, string> ("Plant", CompanyName),
                new Tuple<string, string> ("Report ID", Report.ID.ToString()),
                new Tuple<string, string> ("Report code", Report.Code),
                new Tuple<string, string> ("Report description", Report.Description),
                new Tuple<string, string> ("Report state", Report.Online ? "online" : "offline"),
                new Tuple<string, string> ("User", User),
            };

            foreach (var Row in Rows)
            {
                Row tableRow = table.AddRow();
                var cell = tableRow.Cells[0];
                paragraph = cell.AddParagraph(Row.Item1);
                cell.Format.Font.Bold = true;
                paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
                cell = tableRow.Cells[1];
                paragraph = cell.AddParagraph(Row.Item2);
                paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
            }

            paragraph = Container.AddParagraph("terza riga");
            paragraph.Format.SpaceBefore = 20;

            Container.AddPageBreak();
            addContent(Container, pageWidth);

            await Task.CompletedTask;
        }

        private void addContent(MigraDoc.DocumentObjectModel.Section section, float pageWidth)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            int columnIndex;
            for (columnIndex = 0; columnIndex < Report.Columns.Count; columnIndex++)
            {
                var column = table.AddColumn();
                column.Width = pageWidth / Report.Columns.Count;
            }

            Row row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            columnIndex = 0;
            foreach (var Column in Report.Columns)
            {
                var cell = row.Cells[columnIndex++];
                var paragraph = cell.AddParagraph(Column);
                cell.Format.Font.Bold = true;
                cell.Format.Font.Size = 6;
                paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
            }

            foreach(var Row in Report.Rows)
            {
                row = table.AddRow();
                columnIndex = 0;
                foreach (var Value in Row.Record)
                {
                    var cell = row.Cells[columnIndex++];
                    cell.Format.Font.Size = 6;
                    var paragraph = cell.AddParagraph(Value);
                    paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
                }
            }
        }

        protected override Task OnRenderAttachmentsAsync()
        {
            return Task.CompletedTask;
        }

        protected override Task OnNewSectionAsync()
        {
            PageSetup setup = Container.Document.DefaultPageSetup.Clone();
            setup.PageFormat = Format;
            PageSetup.GetPageSize(Format, out var pageWidth, out var pageHeight);
            setup.PageWidth = pageWidth;
            setup.PageHeight = pageHeight;
            setup.Orientation = Orientation;
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