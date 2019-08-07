using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Api.PDF.Helper;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.Prototypes.Logic.PDF
{
    public class PrototypePDF : CheckListPDF
    {
        public PrototypePDF(
            long CompanyID,
            string CompanyName,
            string TimeZone,
            string Repository,
            PrototypeVersions Prototype,
            ICheckList CheckList,
            IStorageService StorageService,
            IConfigurationService ConfigurationService
        ) : base(CompanyID, CompanyName, TimeZone, Repository, CheckList, StorageService, ConfigurationService)
        {
            PrototypeVersion = Prototype;
        }

        private PrototypeVersions PrototypeVersion { get; }

        protected override async Task OnGenerateAsync()
        {
            await base.OnGenerateAsync();

            var Prototype = ((Prototype)Model);
            var Signatures = PrototypeVersion
                .PrototypeVersionsHistory
                .LastOrDefault(h => h.State == VersionState.Approved)?
                .WorkFlowApplications?
                .WorkFlowApplicationSteps
                .OrderBy(h => h.Step)
                .ThenBy(h => h.ID)
                .ToList();

            if (Signatures != null && Signatures.Any())
            {
                Container.AddPageBreak();

                Paragraph paragraph = Container.AddParagraph("Approval workflow").SetFont(TitleFont1);
                paragraph.Format.Alignment = ParagraphAlignment.Center;

                var Table = new PdfPTable(7) { WidthPercentage = 100 };

                Table.AddCell(new PdfPCell(new Paragraph().SetText("User", GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Date", GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Time", GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature step", GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature reason", GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature comment", GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature result", GenericFont).Padding(10)));
                Table.CompleteRow();

                foreach (var Signature in Signatures)
                {
                    Table.AddCell(new PdfPCell(new Paragraph().SetText($"{Signature.User} - {Signature.Users?.Description}", SmallFont).Padding(10).SetAlignment(ParagraphAlignment.Center)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Date?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd"), SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Date?.ToLocalTime(TimeZone).ToString("HH:mm UTC zzz"), SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText((Signature.Step + 1).ToString(), SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Reason)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Comment, SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(!Signature.Approved.HasValue ? "Pending" : Signature.Approved.Value ? "Approved" : "Rejected", PDFBase.SmallFont).Padding(10)) { HorizontalAlignment = ParagraphAlignment.Center, VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.CompleteRow();
                }

                Container.Add(Table);
            }
        }

        protected override FirstPage GenerateFirstPage()
        {
            return new FirstPage(this, $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}", PrototypeVersion.Description, null, new List<Tuple<string, string>> {
                new Tuple<string, string>("Plant", CompanyName),
                new Tuple<string, string>("Department", PrototypeVersion.Prototypes.Departments.Name),
                new Tuple<string, string>("Prototype type", PrototypeVersion.Prototypes.PrototypeTypes.Name),
                new Tuple<string, string>("Prototype name", PrototypeVersion.Prototypes.Name),
                new Tuple<string, string>("Prototype version", PrototypeVersion.Version.ToJson()),
                new Tuple<string, string>("Prototype state", PrototypeVersion.VersionState),
                new Tuple<string, string>("Valid from", PrototypeVersion.ValidFrom?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm UTC zzz") ?? "N/A"),
                new Tuple<string, string>("Valid until", PrototypeVersion.ValidUntil?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm UTC zzz") ?? "N/A")
            });
        }

        protected override Task<Template> GenerateTemplateAsync() => Task.FromResult(new Template(this,
            $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version} - {PrototypeVersion.VersionState}"));

        public override string GetFieldValue(ICheckListField Field) => new string('\n', Field.LinesCount);

        public override bool IsPrototype() => true;

        public override void RenderGroup(ICheckListGroup Group)
        {
            Group.CheckList = Model;
            ((Models.CheckLists.Group)Group).AddIterations(Math.Max(Group.PrototypeIterations, 1));
        }
    }
}
