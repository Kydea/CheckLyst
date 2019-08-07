using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Services;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

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

            var Header = Model.Header;
            if (Header != null && Header is ICheckListGroup)
                await new Group(this, Header).RenderAsync();

            foreach (var Element in Model.Pages)
            {
                Container.AddPageBreak();
                await new Page(this, Element).RenderAsync();
            }
        }

        protected override async Task OnRenderAttachmentsAsync()
        {
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
                    if (Attachment.ContentType == "images")
                    {
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
                    else if (Attachment.ContentType == "images")
                    {
                        Container.AddPageBreak();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await Attachment.Content.CopyToAsync(ms);
                            PdfDocument Document = PdfReader.Open(ms, PdfDocumentOpenMode.Import);

                            for (int PageIndex = 0; PageIndex < Document.PageCount; PageIndex++)
                            {
                                var Page = Document.Pages[PageIndex];
                                PdfDocument.AddPage(Page);
                            }
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
    }
}
