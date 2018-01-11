using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public class CheckListPDF : PDF.PDFBase
    {
        public ICheckList Model;
        public string PrototypeDescription = "", InstanceCode = "";

        public CheckListPDF(ICheckList Model) : base("CheckList")
        {
            this.Model = Model;
        }

        protected override void OnGenerate()
        {
            PdfWriter.PageEvent = new HeaderAndFooter(this);
            Document.SetMargins(40, 40, 120, 120);

            NewPage();

            //First Page

            Image Logo = Image.GetInstance(System.Web.Hosting.HostingEnvironment.MapPath("/Content/PDF/Janssen.png"));
            Logo.ScaleToFit(Document.PageSize.Width / 3, 400f);
            Logo.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            Document.Add(Logo);

            string FirstText = "", SecondText = "", PrototypeType = "", Department = "", State = "", ValidFrom = "", ValidUntil = "", InstanceCreationDate = "", InstanceApprovationDate = "";
            bool IsInstance = true; //TODO PDF togliere true

            // //TODO PDF
            //if (Model is Prototype)
            //{
            //    var Prototype = ((Prototype)Model);

            //    FirstText = Prototype.Description + " ( V. " + Prototype.Version + " )";
            //    SecondText = Prototype.Code;

            //    PrototypeType = Model.PrototypeType;
            //    Department = Prototype.Department;
            //    PrototypeDescription = Prototype.Description;
            //    State = Prototype.State;
            //    ValidFrom = Prototype.ValidFrom?.ToString("dd/MM/yyyy") ?? ValidFrom;
            //    ValidUntil = Prototype.ValidUntil?.ToString("dd/MM/yyyy") ?? ValidUntil;

            //    IsInstance = false;
            //}
            //else if (Model is Instance)
            //{
            //    var Instance = ((Instance)Model);
            //    SecondText = Instance.Key.ToString();
            //    InstanceCode = SecondText;

            //    var Prototype =
            //        DbContext
            //            .Prototypes
            //            .FirstOrDefault(x => x.Code == Instance.Key.Prototype && x.Versions.Any(y => y.Version == Instance.Key.Version));

            //    PrototypeDescription = FirstText = (Prototype?.Description ?? FirstText) + " ( V. " + Instance.Key.Version + " )";
            //    PrototypeType = Prototype?.PrototypeType ?? PrototypeType;
            //    Department = Prototype?.Department ?? Department;

            //    State = Instance.State;

            //    var InstanceData =
            //        DbContext
            //        .Instances
            //        .FirstOrDefault(
            //                x =>
            //                    x.Prototype == Instance.Key.Prototype &&
            //                    x.Version == Instance.Key.Version &&
            //                    x.Number == Instance.Key.Number &&
            //                    x.Year == Instance.Key.Year
            //            );

            //    InstanceCreationDate = InstanceData?.CreatedOn.ToString("dd/MM/yyyy") ?? "";
            //    InstanceApprovationDate =
            //        DbContext
            //            .History
            //            .Where(
            //                    x =>
            //                        x.Prototype == Instance.Key.Prototype &&
            //                        x.Version == Instance.Key.Version &&
            //                        x.Number == Instance.Key.Number &&
            //                        x.Year == Instance.Key.Year &&
            //                        x.State == "Confirmed"
            //                )
            //            ?.ToList()
            //            ?.LastOrDefault()
            //            ?.Date.ToString("dd/MM/yyyy") 
            //            ?? "";

            //    IsInstance = true;
            //}
            //else
            //    throw new NotImplementedException();

            Paragraph Text = new Paragraph(new Phrase("\n" + FirstText + "\n"));
            Text.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            Document.Add(Text);

            Text = new Paragraph(new Phrase(SecondText + "\n\n\n"));
            Text.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            Document.Add(Text);

            PdfPTable Table = new PdfPTable(2);

            Table.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

            if (!String.IsNullOrEmpty(PrototypeType))
            {
                Table.AddCell(new PdfPCell(new Paragraph("Prototype Type", PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(PrototypeType, PDFSection.GenericFont)) { Padding = 10 });
            }

            if (!String.IsNullOrEmpty(Department))
            {
                Table.AddCell(new PdfPCell(new Paragraph("Department", PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(Department, PDFSection.GenericFont)) { Padding = 10 });
            }

            if (!String.IsNullOrEmpty(State))
            {
                Table.AddCell(new PdfPCell(new Paragraph(IsInstance ? "Instance State" : "State", PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(State, PDFSection.GenericFont)) { Padding = 10 });
            }

            if (!String.IsNullOrEmpty(ValidFrom))
            {
                Table.AddCell(new PdfPCell(new Paragraph("Validation Start", PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(ValidFrom, PDFSection.GenericFont)) { Padding = 10 });
            }

            if (!String.IsNullOrEmpty(ValidUntil))
            {
                Table.AddCell(new PdfPCell(new Paragraph("Validation End", PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(ValidUntil, PDFSection.GenericFont)) { Padding = 10 });
            }

            if (!String.IsNullOrEmpty(InstanceCreationDate))
            {
                Table.AddCell(new PdfPCell(new Paragraph("Instance Creation Date", PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(InstanceCreationDate, PDFSection.GenericFont)) { Padding = 10 });
            }

            if (!String.IsNullOrEmpty(InstanceApprovationDate))
            {
                Table.AddCell(new PdfPCell(new Paragraph("Instance Approvation Date", PDFSection.GenericBoldFont)) { Padding = 10 });
                Table.AddCell(new PdfPCell(new Paragraph(InstanceApprovationDate, PDFSection.GenericFont)) { Padding = 10 });
            }

            Document.Add(Table);

            NewPage();

            //Header
            var Header = Model.Header;
            if (Header != null && Header is ICheckListGroup)
                new Sections.Group(this, (ICheckListGroup)Header).Render();

            //Pages
            foreach (var Element in Model.Pages)
            {
                Document.NewPage();
                CurrentRenderingY = 0;
                new Sections.Page(this, (ICheckListPage)Element).Render();
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

    public class HeaderAndFooter : PdfPageEventHelper
    {

        CheckListPDF PDF;
        PdfTemplate headerTemplate, footerTemplate;

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        #endregion

        public HeaderAndFooter(CheckListPDF PDF)
        {
            this.PDF = PDF;
            headerTemplate = PDF.PdfContentByte.CreateTemplate(40, 40);
            footerTemplate = PDF.PdfContentByte.CreateTemplate(40, 40);
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            base.OnEndPage(writer, document);

            Phrase Description = new Phrase(PDF.PrototypeDescription + "\n\n" + PDF.InstanceCode, PDFSection.GenericFont);

            //Create Tablele object
            PdfPTable Table = new PdfPTable(1);
            PdfPCell Cell = new PdfPCell(writer.PageNumber != 1 ? Description : null);
            String text = "Page " + writer.PageNumber + " of ";

            PDF.PdfContentByte.BeginText();
            PDF.PdfContentByte.SetFontAndSize(PDFSection.BaseFont, 12);
            PDF.PdfContentByte.SetTextMatrix(document.PageSize.GetRight(105), document.PageSize.GetBottom(55));
            PDF.PdfContentByte.ShowText(text);
            PDF.PdfContentByte.EndText();

            PDF.PdfContentByte.BeginText();
            PDF.PdfContentByte.SetFontAndSize(PDFSection.BaseFont, 12);
            PDF.PdfContentByte.SetTextMatrix(document.PageSize.GetLeft(45), document.PageSize.GetBottom(55));
            PDF.PdfContentByte.ShowText(DateTime.UtcNow.ToString("dd/MM/yyyy"));
            PDF.PdfContentByte.EndText();

            float len = PDFSection.BaseFont.GetWidthPoint(text, 12);
            PDF.PdfContentByte.AddTemplate(footerTemplate, document.PageSize.GetRight(105) + len, document.PageSize.GetBottom(55));

            Cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;

            Cell.Border = 0;

            Table.AddCell(Cell);

            Table.TotalWidth = document.PageSize.Width - 40f;
            Table.WidthPercentage = 100;
            Table.WriteSelectedRows(0, -1, 40, document.PageSize.Height - 60, writer.DirectContent);

            if (writer.PageNumber != 1)
            {
                PDF.PdfContentByte.MoveTo(40, document.PageSize.Height - 100);
                PDF.PdfContentByte.LineTo(document.PageSize.Width - 40, document.PageSize.Height - 100);
                PDF.PdfContentByte.Stroke();
            }

            PDF.PdfContentByte.MoveTo(40, document.PageSize.GetBottom(90));
            PDF.PdfContentByte.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(90));
            PDF.PdfContentByte.Stroke();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(PDFSection.BaseFont, 12);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber).ToString());
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(PDFSection.BaseFont, 12);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber).ToString());
            footerTemplate.EndText();
        }
    }
}