using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.PDF.Helper;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Services;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using CheckListManager.Common.Core.Helpers;
using System.Linq;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class CheckListPDF : PDFBase
    {
        public long CompanyID { get; }
        public string CompanyName { get; }
        public string TimeZone { get; }
        public string Repository { get; }
        public ICheckList Model { get; }

        public IStorageService StorageService { get; }
        public IConfigurationService ConfigurationService { get; }
        private Dictionary<string, Models.StorageFile> Attachments { get; } = new Dictionary<string, Models.StorageFile>();
        private Dictionary<string, int> AttachmentIndexes { get; } = new Dictionary<string, int>();

        public CheckListPDF(long CompanyID, string CompanyName, string TimeZone, string Repository, ICheckList Model, IStorageService StorageService, IConfigurationService ConfigurationService) : base("CheckList")
        {
            this.CompanyID = CompanyID;
            this.CompanyName = CompanyName;
            this.TimeZone = TimeZone;
            this.Repository = Repository;
            this.Model = Model;
            this.StorageService = StorageService;
            this.ConfigurationService = ConfigurationService;
        }

        public async Task<int> AddAttachmentAsync(string Bucket, string Key)
        {
            var CompleteKey = Bucket + Key;
            if (!AttachmentIndexes.ContainsKey(CompleteKey))
            {
                var StorageFile = await StorageService.GetAsync(Bucket, Key);
                Attachments[CompleteKey] = StorageFile;
                AttachmentIndexes[CompleteKey] = Attachments.Count;
            }
            return AttachmentIndexes[CompleteKey];
        }

        protected abstract FirstPage GenerateFirstPage();

        protected abstract Task<Template> GenerateTemplateAsync();

        public abstract string GetFieldValue(ICheckListField Field);

        public abstract bool IsPrototype();

        protected override async Task OnGenerateAsync()
        {
            await GenerateFirstPage().RenderAsync();
            await AddSectionAsync();

            await OnRenderAttachmentsListAsync();

            var Header = Model.Header;
            if (Header != null && Header is ICheckListGroup)
                await new Group(this, Header).RenderAsync();

            foreach (var Element in Model.Pages)
            {
                Container.AddPageBreak();
                await new Page(this, Element).RenderAsync();
            }
        }

        private Task<List<ICheckListItem>> GetAttachmentsList()
        {
            List<ICheckListItem> AttachmentsList = new List<ICheckListItem>();
            bool IsAttachment(ICheckListItem Item) => Item is ICheckListAttachmentBase; 

            void Traverse(ICheckListGroup Group)
            {
                Group.Items.ForEach(Item =>
                {
                    if (IsAttachment(Item))
                        AttachmentsList.Add(Item);
                    else if (Item is ICheckListGroup checkListGroup)
                        Traverse(checkListGroup);
                });
            };

            if (Model.Header != null && Model.Header is ICheckListGroup Header)
                Traverse(Header);

            foreach (var Element in Model.Pages)
                Element.Groups.ForEach(Traverse);

            return Task.FromResult(AttachmentsList);
        }

        private async Task OnRenderAttachmentsListAsync()
        {
            var Attachments = await GetAttachmentsList();

            if (Attachments.Any())
            {
                Container.AddPageBreak();
                var Paragraph = Container.AddParagraph("Attachments").SetFont(TitleFont1);
                Paragraph.Format.SpaceAfter = 10;

                var Table = new PdfPTable(2);

                int Index = 1;
                foreach (var Attachment in Attachments)
                {
                    Table.AddCell(new PdfPCell(new Paragraph().SetText($"Attachment #{Index}", GenericFont).Padding(5)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Attachment.Description, GenericFont).Padding(5)));
                    Index++;
                }

                Container.Add(Table);
                await AddSectionAsync();
            }
        }

        protected override async Task OnRenderAttachmentsAsync()
        {
            if (!Attachments.Any())
                return;

            DocumentBuilder.RenderPdfDocument();
            await this.AddSectionAsync();
            Container.RemovePageCount();

            int Index = 1;
            foreach (var Attachment in Attachments.Values)
            {
                Container.AddPageBreak();
                Paragraph Paragraph = Container.AddParagraph();
                Paragraph.SetFont(TitleFont1);
                Paragraph.AddText($"Attachment {Index++}");
                Paragraph.Format.SpaceAfter = 10;

                try
                {
                    if (Attachment.ContentType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Paragraph = Container.AddParagraph();

                        Bitmap Image;
                        byte[] Data;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await Attachment.Content.CopyToAsync(ms);
                            Image = new Bitmap(ms);
                            Data = ms.ToArray();
                        }

                        var PDFImage = Paragraph.AddImage(Data);
                        var MaxWidth = Container.PageSetup.PageWidth - Container.PageSetup.LeftMargin - Container.PageSetup.LeftMargin;
                        Paragraph.Format.SpaceAfter = 10;

                        if (Image.Width * .4 > MaxWidth)
                        {
                            PDFImage.LockAspectRatio = true;
                            PDFImage.Width = Container.PageSetup.PageWidth - Container.PageSetup.LeftMargin - Container.PageSetup.LeftMargin;
                        }
                        else
                            PDFImage.ScaleWidth = .4;
                    }
                    else if (Attachment.ContentType.Equals("application/pdf", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await Attachment.Content.CopyToAsync(ms);
                            PdfDocument Document = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
                            DocumentBuilder.AddRange(Enumerable.Range(0, Document.PageCount).Select(index => Document.Pages[index]));
                        }
                    }
                }
                catch (Exception Exception)
                {
                    Paragraph ExceptionParagraph = Container.AddParagraph();
                    ExceptionParagraph.SetFont(GenericFont);
                    ExceptionParagraph.AddText($"Error: {Exception.Message}");
                    ExceptionParagraph.Format.SpaceAfter = 10;
                }
            }
        }

        protected override Task OnNewSectionAsync() => GenerateTemplateAsync();

        public virtual void RenderGroup(ICheckListGroup Group)
        {
        }

        protected void GenerateSignatureBook(List<WorkFlowApplicationSteps> Signatures)
        {
            if (Signatures != null && Signatures.Any())
            {
                Container.AddPageBreak();

                Paragraph paragraph = Container.AddParagraph("Approval workflow").SetFont(TitleFont1);
                paragraph.Format.SpaceAfter =    10;

                var Table = new PdfPTable(7) { WidthPercentage = 100 };

                Table.AddCell(new PdfPCell(new Paragraph().SetText("User", BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Date", BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Time", BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature step", BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature reason", BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature comment", BoldFont).Padding(5)));
                Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature result", BoldFont).Padding(5)));
                Table.CompleteRow();

                foreach (var Signature in Signatures)
                {
                    Table.AddCell(new PdfPCell(new Paragraph().SetText($"{Signature.User} - {Signature.UserDescription}", GenericFont).Padding(5)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Date?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd"), GenericFont).Padding(5)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Date?.ToLocalTime(TimeZone).ToString("HH:mm UTC zzz"), GenericFont).Padding(5)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText((Signature.Step + 1).ToString(), GenericFont).Padding(5)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Reason, GenericFont).Padding(5)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(Signature.Comment, GenericFont).Padding(5)));
                    Table.AddCell(new PdfPCell(new Paragraph().SetText(!Signature.Approved.HasValue ? "Pending" : Signature.Approved.Value ? "Approved" : "Rejected", GenericFont).Padding(5)));
                    Table.CompleteRow();
                }

                Container.Add(Table);
            }
        }
    }
}
