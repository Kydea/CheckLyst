using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Api.PDF.Helper;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.PDF
{
    public class PrototypePDF : CheckListPDF
    {
        public PrototypePDF(long CompanyID, string CompanyName, string TimeZone, string Repository, PrototypeVersions Prototype, ICheckList CheckList) : base(CompanyID, CompanyName, TimeZone, Repository, CheckList)
        {
            PrototypeVersion = Prototype;
        }

        private PrototypeVersions PrototypeVersion { get; }

        protected override void OnGenerate()
        {
            base.OnGenerate();

            var Prototype = ((Prototype)Model);
            var Signatures = PrototypeVersion.PrototypeVersionsHistory.LastOrDefault(h => h.State == VersionState.Approved)?.WorkFlowApplications?.WorkFlowApplicationSteps.OrderBy(h => h.Step).OrderBy(h => h.ID);

            if (Signatures != null && Signatures.Count() > 0)
            {
                Container.AddPageBreak();

                Paragraph paragraph = Container.AddParagraph("Approval workflow \n\n").SetFont(PDFSection.FirstHeaderFont);
                paragraph.Format.Alignment = ParagraphAlignment.Center;

                var Table = new PdfPTable(7) { WidthPercentage = 100 };

                Table.AddCell(new PdfPCell(new Paragraph().SetText("User", PDFSection.GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Date", PDFSection.GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Time", PDFSection.GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature step", PDFSection.GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature reason", PDFSection.GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature comment", PDFSection.GenericFont).Padding(10)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature result", PDFSection.GenericFont).Padding(10)));
                Table.CompleteRow();

                foreach (var Signature in Signatures)
                {
                    Table.AddCell(new PdfPCell(new Paragraph().SetText($"{Signature.User} - {Signature.Users?.Description}", PDFSection.SmallFont).Padding(10).SetAlignment(ParagraphAlignment.Center)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Date?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd"), PDFSection.SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Date?.ToLocalTime(TimeZone).ToString("HH:mm UTC zzz"), PDFSection.SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText((Signature.Step + 1).ToString(), PDFSection.SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Reason)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Comment, PDFSection.SmallFont).Padding(10)) { VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(!Signature.Approved.HasValue ? "Pending" : Signature.Approved.Value ? "Approved" : "Rejected", PDFSection.SmallFont).Padding(10)) { HorizontalAlignment = ParagraphAlignment.Center, VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center });
                    Table.CompleteRow();
                }

                Container.Add(Table);
            }
        }

        protected override FirstPage GenerateFirstPage()
        {
            return new FirstPage(this, $"{PrototypeVersion.Prototypes.Name}", $"Version {PrototypeVersion.Version}", PrototypeVersion.Description, new List<Tuple<string, string>> {
                new Tuple<string, string>("Plant", CompanyName),
                new Tuple<string, string>("Department", PrototypeVersion.Prototypes.Departments.Name),
                new Tuple<string, string>("Prototype type", PrototypeVersion.Prototypes.PrototypeTypes.Name),
                new Tuple<string, string>("State", PrototypeVersion.VersionState),
                new Tuple<string, string>("Valid from", PrototypeVersion.ValidFrom?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm UTC zzz")),
                new Tuple<string, string>("Valid until", PrototypeVersion.ValidUntil?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm UTC zzz"))
            });
        }

        protected override Template GenerateTemplate() => new Template(this, $"{PrototypeVersion.Prototypes.Departments.Name}.{PrototypeVersion.Version}\n{PrototypeVersion.Description}");

        public override string GetFieldValue(ICheckListField Field) => "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";

        public override bool IsPrototype() => true;

        public override void RenderGroup(ICheckListGroup Group)
        {
            Group.CheckList = Model;
            ((Models.CheckLists.Group)Group).AddIterations(Math.Max(Group.PrototypeIterations, 1));
        }
    }
}
