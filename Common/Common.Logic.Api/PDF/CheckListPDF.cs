using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Api.PDF.Sections;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class CheckListPDF : PDFBase
    {
        public long CompanyID { get; }
        public string CompanyName { get; }
        public string TimeZone { get; }
        public string Repository { get; }

        public ICheckList Model { get; }

        public string PrototypeDescription = "", InstanceCode = "";

        public CheckListPDF(long CompanyID, string CompanyName, string TimeZone, string Repository, ICheckList Model) : base("CheckList")
        {
            this.CompanyID = CompanyID;
            this.CompanyName = CompanyName;
            this.TimeZone = TimeZone;
            this.Repository = Repository;
            this.Model = Model;
        }

        protected abstract FirstPage GenerateFirstPage();

        protected abstract Template GenerateTemplate();

        public string GetFieldValue(Item Item) => GetFieldValue((ICheckListField)Item.Element);

        public abstract string GetFieldValue(ICheckListField Field);

        public abstract bool IsPrototype();

        protected override void OnGenerate()
        {
            PdfWriter.PageEvent = GenerateTemplate();
            Document.SetMargins(40, 40, 120, 120);

            GenerateFirstPage().Render();

            //Header
            var Header = Model.Header;
            if (Header != null && Header is ICheckListGroup)
                new Group(this, Header).Render();

            //Pages
            foreach (var Element in Model.Pages)
            {
                Document.NewPage();
                CurrentRenderingY = 0;
                new Page(this, Element).Render();
            }

            //TODO PDF
            //if (Model is Prototype)
            //{
            //    var Prototype = ((Prototype)Model);
            //    var Version = DbContext.Versions.FirstOrDefault(x => x.Prototype == Prototype.Code && x.Version == Prototype.Version);
            //    var Signatures = PrototypeSignatures.Load(Version, PrototypeSignatureBookActions.TO_ACTIVE, true);

            //    if (Signatures.Count > 0)
            //    {
            //        Document.NewPage();

            //        Text = new Paragraph(new Phrase("History \n\n", PDFSection.FirstHeaderFont));
            //        Text.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            //        Document.Add(Text);

            //        Table = new PdfPTable(7);

            //        Table.AddCell(new PdfPCell(new Paragraph("User", PDFSection.GenericFont)) { Padding = 10 });
            //        Table.AddCell(new PdfPCell(new Paragraph("Date", PDFSection.GenericFont)) { Padding = 10 });
            //        Table.AddCell(new PdfPCell(new Paragraph("Time", PDFSection.GenericFont)) { Padding = 10 });
            //        Table.AddCell(new PdfPCell(new Paragraph("Signature Step", PDFSection.GenericFont)) { Padding = 10 });
            //        Table.AddCell(new PdfPCell(new Paragraph("Signature Reason", PDFSection.GenericFont)) { Padding = 10 });
            //        Table.AddCell(new PdfPCell(new Paragraph("Signature Comment", PDFSection.GenericFont)) { Padding = 10 });
            //        Table.AddCell(new PdfPCell(new Paragraph("Signature Result", PDFSection.GenericFont)) { Padding = 10 });


            //        foreach (var Signature in Signatures)
            //        {
            //            Table.AddCell(new PdfPCell(new Paragraph(Signature.User + " - " + Signature.Users.Description, PDFSection.SmallFont)) { Padding = 10, VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            //            Table.AddCell(new PdfPCell(new Paragraph(Signature.Date.ToString("dd/MM/yyyy"), PDFSection.SmallFont)) { Padding = 10, VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            //            Table.AddCell(new PdfPCell(new Paragraph(Signature.Date.ToString("hh:mm"), PDFSection.SmallFont)) { Padding = 10, VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            //            Table.AddCell(new PdfPCell(new Paragraph(Signature.Step.ToString(), PDFSection.SmallFont)) { Padding = 10, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            //            Table.AddCell(new PdfPCell(new Paragraph(Signature.SignatureReason, PDFSection.SmallFont)) { Padding = 10, VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            //            Table.AddCell(new PdfPCell(new Paragraph(Signature.Comment, PDFSection.SmallFont)) { Padding = 10, VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            //            Table.AddCell(new PdfPCell(new Paragraph(Signature.Approved ? "Approved" : "Rejected", PDFSection.SmallFont)) { Padding = 10, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
            //        }

            //        Table.WidthPercentage = 100;
            //        Document.Add(Table);
            //    }
            //}
        }
    }
}