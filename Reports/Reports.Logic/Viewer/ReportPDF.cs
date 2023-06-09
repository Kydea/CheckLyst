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
using System.Threading;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using CheckListManager.Reports.Logic.Detail;
using CheckListManager.Common.Logic.Models;
using static CheckListManager.Common.Logic.Models.DataType;
using AutoMapper;
using CheckListManager.Common.Logic.Models.Expressions.Functions;
using System.Diagnostics.CodeAnalysis;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportPDF : PDFBase
    {
        public long CompanyID { get; }
        public string CompanyName { get; }
        public string TimeZone { get; }
        public string DecimalSeparator { get; }
        public string Repository { get; }
        int FontSize { get; }
        PageFormat Format { get; }
        List<int> ColumnWidths { get; }
        Orientation Orientation { get; set; }
        CancellationToken CancellationToken { get; }
        public string User { get; }
        public ReportRequest Request { get; }
        public ReportResponse Report { get; }
        public DetailResponse Details { get; }
        private MigraDoc.DocumentObjectModel.Tables.Table ReportTable;
        private float PageWidth;


        public IConfigurationService ConfigurationService { get; }

        public ReportPDF(
            long CompanyID,
            string CompanyName,
            string TimeZone,
            string DecimalSeparator,
            string Repository,
            IConfigurationService ConfigurationService,
            int FontSize,
            PageFormat Format,
            Orientation Orientation,
            string User,
            ReportRequest Request,
            ReportResponse Report,
            DetailResponse Details,
            CancellationToken CancellationToken
        ) : base("CheckList")
        {
            this.CompanyID = CompanyID;
            this.CompanyName = CompanyName;
            this.TimeZone = TimeZone;
            this.DecimalSeparator = DecimalSeparator;
            this.Repository = Repository;
            this.ConfigurationService = ConfigurationService;
            this.User = User;
            this.Request = Request;
            this.Report = Report;
            this.Details = Details;
            this.FontSize = FontSize;
            this.Format = Format;
            this.Orientation = Orientation;
            ColumnWidths = Request.ColumnWidths;
            this.CancellationToken = CancellationToken;
        }

        protected override async Task OnGenerateAsync()
        {
            Container.Headers = null;
            Container.PageSetup.TopMargin = 70;

            Paragraph header = Container.Headers.Primary.AddParagraph();
            header.Format.Font = GrayFont;
            header.Format.Font.Size = FontSize;
            header.AddText($"{CompanyName} - {Report.ID} - {Report.Code}");
            header.Format.SpaceBefore = 20;
            header.Format.Alignment = ParagraphAlignment.Right;
            header.Format.Borders.Top = DefaultBorder;
            header.Format.Borders.Bottom = DefaultBorder;
            header.Format.Borders.DistanceFromTop = 10;
            header.Format.Borders.DistanceFromBottom = 10;

            Paragraph paragraph = Container.AddParagraph();

            paragraph.Format.Alignment = ParagraphAlignment.Center;
            Image logo = paragraph.AddImage(new Uri($"{Repository}PDF/Logo-{CompanyID}.png"));

            if (logo != null)
                logo.ScaleWidth = 0.4;

            var table = Container.AddTable();
            table.Borders.Color = Gray;
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            PageWidth = Orientation == Orientation.Portrait ?
                Container.PageSetup.PageWidth - Container.PageSetup.LeftMargin - Container.PageSetup.RightMargin :
                Container.PageSetup.PageHeight - Container.PageSetup.TopMargin - Container.PageSetup.BottomMargin;
            var column = table.AddColumn();
            column.Width = PageWidth * .3;

            column = table.AddColumn();
            column.Width = PageWidth * .7;

            var Rows = new List<Tuple<string, string>> {
                new Tuple<string, string> ("Plant", CompanyName),
                new Tuple<string, string> ("Report ID", Report.ID.ToString()),
                new Tuple<string, string> ("Report code", Report.Code),
                new Tuple<string, string> ("Report description", Report.Description),
                new Tuple<string, string> ("Report state", Report.Online ? "online" : "offline"),
                new Tuple<string, string> ("Report extracted by", User),
            };

            if (Request.Parameters != null && Request.Parameters.Any(p => !string.IsNullOrEmpty(p.Value)))
            {
                var parameters = Request.Parameters?.ToDictionary(k => k.Code, v => v);
                Rows.Add(new Tuple<string, string>("Parameters", null));
                foreach (var clause in Details.Clauses)
                    if (parameters.TryGetValue(clause.ParameterName, out var parameter) && !string.IsNullOrEmpty(parameter.Value))
                    {
                        var dataType = FromName(clause.ParameterType);
                        dataType.TryParse(parameter.Value, out var value);
                        var formattedValue =
                            dataType == DataType.Date ? DateTimeOffsetHelper.Format((DateTimeOffset)value, TimeZone, DateTimeOffsetHelper.DateFormat) :
                            dataType == DataType.DateTime ? DateTimeOffsetHelper.Format((DateTimeOffset)value, TimeZone) :
                            dataType == Number ? DecimalHelper.Format((decimal)value, DecimalSeparator) :
                            value.ToString();
                        Rows.Add(new Tuple<string, string>(clause.Label, formattedValue));
                    }
            }

            foreach (var Row in Rows)
            {
                if (CancellationToken.IsCancellationRequested) Thread.CurrentThread.Abort();
                Row tableRow = table.AddRow();
                var cell = tableRow.Cells[0];
                paragraph = cell.AddParagraph(Row.Item1);
                cell.Format.Font.Bold = true;
                cell.Format.Font.Color = DarkGray;
                cell.Format.Font.Size = FontSize;
                paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
                if (Row.Item2 == null)
                {
                    tableRow.Borders.Width = 0;
                    cell.MergeRight = 1;
                    cell.Format.Alignment = ParagraphAlignment.Center;
                }
                else
                {
                    cell = tableRow.Cells[1];
                    cell.Format.Font.Size = FontSize;
                    paragraph = cell.AddParagraph(Row.Item2);
                    paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
                }
            }

            Container.AddPageBreak();
            AddContent(Container);

            await Task.CompletedTask;
        }

        private void AddContent(MigraDoc.DocumentObjectModel.Section section)
        {
            ReportTable = section.AddTable();
            ReportTable.Borders.Color = Gray;
            ReportTable.Borders.Width = 0.25;
            ReportTable.Borders.Left.Width = 0.5;
            ReportTable.Borders.Right.Width = 0.5;
            ReportTable.Rows.LeftIndent = 0;

            int columnIndex;
            if (ColumnWidths == null || ColumnWidths.Count == 1 && ColumnWidths[0] == 0)
            {
                var column = ReportTable.AddColumn();
                var firstColumnSize = Unit.FromPoint(FontSize) * (Math.Floor(Math.Log10(Report.Rows.Count())) + 1) + 10;
                column.Width = firstColumnSize;

                for (columnIndex = 0; columnIndex < Report.Columns.Count; columnIndex++)
                {
                    column = ReportTable.AddColumn();
                    column.Width = (PageWidth - firstColumnSize) / Report.Columns.Count;
                }
            }
            else
                for (columnIndex = 0; columnIndex < Report.Columns.Count + 1; columnIndex++)
                {
                    var column = ReportTable.AddColumn();
                    column.Width = ColumnWidths[columnIndex];
                }

            Row row = ReportTable.AddRow();
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            columnIndex = 1;
            foreach (var Column in Report.Columns)
            {
                var cell = row.Cells[columnIndex++];
                var paragraph = cell.AddParagraph(Column);
                cell.Format.Font.Bold = true;
                cell.Format.Font.Size = FontSize;
                cell.Shading.Color = LightGray;
                paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
            }

            var index = 1;
            foreach (var Row in Report.Rows)
            {
                row = ReportTable.AddRow();

                var cell = row.Cells[0];
                cell.Format.Font.Size = FontSize;
                cell.Shading.Color = LightGray;
                if (!Row.Total)
                {
                    var paragraph = cell.AddParagraph(index++.ToString());
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                    paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
                }

                columnIndex = 1;
                foreach (var Value in Row.Record)
                {
                    cell = row.Cells[columnIndex++];
                    cell.Format.Font.Size = FontSize;
                    if (Row.Total)
                        cell.Shading.Color = LightGray;
                    var link = Value != null && (Value.StartsWith("http://") || Value.StartsWith("https://"));
                    if (link)
                    {
                        var paragraph = cell.AddParagraph();
                        var hyperlink = paragraph.AddHyperlink(Value, HyperlinkType.Url);
                        var text = hyperlink.AddFormattedText("(click)");
                        text.Style = StyleNames.Hyperlink;
                    }
                    else
                    {
                        var softhyphen = "\xad";
                        var paragraph = cell.AddParagraph(string.Join("", Value.ToCharArray().Select(c => softhyphen + c)));
                        paragraph.Format.SpaceAfter = paragraph.Format.SpaceBefore = paragraph.Format.LeftIndent = paragraph.Format.RightIndent = 5;
                    }
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
            if (ColumnWidths == null || ColumnWidths.Count == 1 && ColumnWidths[0] == 0)
            {
                setup.PageFormat = Format;
                PageSetup.GetPageSize(Format, out var pageWidth, out var pageHeight);
                setup.PageWidth = pageWidth;
                setup.PageHeight = pageHeight;
            }
            else
            {
                var pageWidth = ColumnWidths.Sum() + 80;
                setup.PageWidth = pageWidth;
                if (Orientation == Orientation.Portrait)
                    setup.PageHeight = pageWidth * 3 / 2;
                else
                    setup.PageHeight = pageWidth * 2 / 3;
                Orientation = Orientation.Portrait;
            }
            setup.Orientation = Orientation;
            setup.LeftMargin = setup.RightMargin = Unit.FromPoint(40);
            setup.TopMargin = Unit.FromPoint(120);
            setup.BottomMargin = Unit.FromPoint(80);
            setup.FooterDistance = Unit.FromPoint(61);
            setup.HeaderDistance = Unit.FromPoint(0);
            setup.DifferentFirstPageHeaderFooter = true;

            Container.PageSetup = setup;

            Style style = Container.Document.Styles[StyleNames.Normal];
            style.Font = GenericFont.Clone();

            Paragraph footer = Container.Footers.Primary.AddParagraph();
            footer.Tag = "PageCount";
            footer.Format.Font = GrayFont;
            footer.Format.Font.Size = FontSize;
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