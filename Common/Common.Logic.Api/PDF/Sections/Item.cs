using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Core.Helpers;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel;
using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel.Shapes;
using System.IO;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Item : CheckListSection
    {
        public Item(CheckListPDF PDFBase, ICheckListItem Item, ICheckListItem Next, bool FirstOfGroup, int Indentation = 1) : base(PDFBase, Item, Indentation)
        {
        }

        private void DrawSignature(string Description, bool Valid, string User = null, string Date = null, string Time = null, string Signature = null)
        {
            PdfPTable Table = new PdfPTable(4) { WidthPercentage = 100 };

            Table.AddCell(new PdfPCell(new Paragraph().SetText("Signed by").SetFont(PDFBase.BoldFont).Padding(5)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText("Date").SetFont(PDFBase.BoldFont).Padding(5)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText("Time").SetFont(PDFBase.BoldFont).Padding(5)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature").SetFont(PDFBase.BoldFont).Padding(5)));
            Table.CompleteRow();

            Table.AddCell(new PdfPCell(new Paragraph().SetText(User).Padding(5)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText(Date).Padding(5)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText(Time).Padding(5)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText(Date == null ? " " : Signature).Padding(5)));
            Table.CompleteRow();

            if (!Valid && !CheckListPDF.IsPrototype())
                Table.BorderColor = PDFBase.Red;

            PDFBase.Container.Add(Table);
            DrawText("\n");
        }

        private void DrawInstanceLink(bool Valid, string InstanceCode = null, string InstanceDescription = null, string InstanceState = null)
        {
            PdfPTable Table = new PdfPTable(1) { WidthPercentage = 100 };

            Table.AddCell(new PdfPCell(new Paragraph().SetText("Instance code").SetFont(PDFBase.BoldFont).Padding(5)));
            Table.CompleteRow();

            Table.AddCell(new PdfPCell(new Paragraph().SetText(InstanceCode).Padding(5)));
            Table.CompleteRow();

            if (!Valid && !CheckListPDF.IsPrototype())
                Table.BorderColor = PDFBase.Red;

            PDFBase.Container.Add(Table);
            DrawText("\n");
        }

        public override async Task RenderAsync()
        {
            if (!CheckListPDF.IsPrototype() && (Element.Hidden || (Element is ICheckListIteration iteration && iteration.Removed))) return;

            if (Element is ICheckListGroup GroupElement)
            {
                if (GroupElement.ShowAsTable)
                    await new Table((CheckListPDF)PDFBase, GroupElement, Indentation + 1).RenderAsync();
                else
                    await new Group((CheckListPDF)PDFBase, GroupElement, Indentation + 1).RenderAsync();
            }
            else await base.RenderAsync();

            string Description = null;
            if (Element is ICheckListItem CheckListItem)
                Description = CheckListPDF.GetItemDescription(CheckListItem);

            if (Element is ICheckListText)
            {
                if (!string.IsNullOrEmpty(Description))
                    DrawText(Description, null, 10);
            }
            else if (Element is ICheckListSignature SignatureElement)
            {
                if (!string.IsNullOrEmpty(Description))
                    DrawText(Description, PDFBase.BoldFont, 5);

                if (SignatureElement.History.Count > 0)
                    foreach (var Sign in SignatureElement.History)
                    {
                        var Date = Sign.DateTime.ToLocalTime(CheckListPDF.TimeZone);
                        DrawSignature(string.IsNullOrEmpty(Sign.Note) ? "Signed by" : "Canceled by", SignatureElement.Valid, Sign.User, Date.ToString("yyyy-MM-dd"), Date.ToString("HH:mm:ss"), Sign.Signature);
                    }
                else
                    DrawSignature("Signed by", SignatureElement.Valid);
            }
            else if (Element is ICheckListInstanceLink InstanceLink)
            {
                if (!string.IsNullOrEmpty(Description))
                    DrawText(Description, PDFBase.BoldFont, 5);

                DrawInstanceLink(InstanceLink.Valid, InstanceLink.Record.Code ?? $"{InstanceLink.PrototypeName}.?.?.?", InstanceLink.Record.Description, InstanceLink.Record.State);
            }
            else if (Element is ICheckListField FieldElement)
            {
                if (!string.IsNullOrEmpty(Description))
                    DrawText(Description, PDFBase.BoldFont, 5);

                if (FieldElement.ValueType.TypeName == "Boolean")
                {
                    PdfPTable Table = new PdfPTable(1) { WidthPercentage = 3, HorizontalAlignment = RowAlignment.Left };
                    if (!bool.TryParse(FieldElement.Value, out var Value))
                        Value = false;
                    var Cell = new PdfPCell(new Paragraph().SetText(Value ? "X" : string.Empty).SetFont(PDFBase.GenericFont).Padding(0)).SetHorizontalAlign(ParagraphAlignment.Center);
                    Table.AddCell(Cell);
                    if (!FieldElement.Valid && !CheckListPDF.IsPrototype())
                        Table.BorderColor = PDFBase.Red;
                    if (FieldElement.ReadOnly)
                        Cell.Content.Format.Shading.Color = PDFBase.LightGray;
                    Table.CompleteRow();
                    PDFBase.Container.Add(Table);
                    DrawText(string.Empty);
                }
                else
                {
                    var Paragraph = DrawText(CheckListPDF.GetFieldValue(FieldElement), PDFBase.GenericFont, 10);
                    Paragraph.Format.LeftIndent = 5;
                    Paragraph.Format.RightIndent = 5;
                    Paragraph.Format.Borders.Top = FieldElement.Valid || CheckListPDF.IsPrototype() ? PDFBase.DefaultBorder : PDFBase.RedBorder;
                    Paragraph.Format.Borders.Right = FieldElement.Valid || CheckListPDF.IsPrototype() ? PDFBase.DefaultBorder : PDFBase.RedBorder;
                    Paragraph.Format.Borders.Bottom = FieldElement.Valid || CheckListPDF.IsPrototype() ? PDFBase.DefaultBorder : PDFBase.RedBorder;
                    Paragraph.Format.Borders.Left = FieldElement.Valid || CheckListPDF.IsPrototype() ? PDFBase.DefaultBorder : PDFBase.RedBorder;
                    Paragraph.Format.Borders.DistanceFromLeft = Paragraph.Format.Borders.DistanceFromRight = 5;
                    Paragraph.Format.Borders.DistanceFromTop = Paragraph.Format.Borders.DistanceFromBottom = 5;
                    if (FieldElement.ReadOnly)
                        Paragraph.Format.Shading.Color = PDFBase.LightGray;
                }
            }
            else if (Element is ICheckListLink LinkElement)
            {
                var Paragraph = DrawText(null, PDFBase.GenericFont, 10).AddHyperlink(LinkElement.Url, HyperlinkType.Web).AddFormattedText(string.IsNullOrEmpty(Description) ? "(link)" : Description);
                Paragraph.Font.Underline = Underline.Single;
            }
            else if (Element is ICheckListImage ImageElement)
            {
                try
                {
                    Paragraph Paragraph;

                    if (!string.IsNullOrEmpty(Description))
                    {
                        Paragraph = DrawText(Description, PDFBase.BoldFont, 5);
                        Paragraph.Format.Alignment = ParagraphAlignment.Center;
                    }

                    var Key = ImageElement.GetTarget().Key;
                    if (Key == null) return;

                    Paragraph = PDFBase.Container.AddParagraph();
                    Paragraph.Format.Alignment = ParagraphAlignment.Center;
                    var StorageFile =
                        await CheckListPDF.StorageService.GetAsync(
                            CheckListPDF.ConfigurationService.PrototypesStorageBucket, Key);
                    byte[] Data;
                    Bitmap Image;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await StorageFile.Content.CopyToAsync(ms);
                        Image = new Bitmap(ms);
                        Data = ms.ToArray();
                    }

                    var PDFImage = Paragraph.AddImage(Data);
                    var MaxWidth = PDFBase.Container.PageSetup.PageWidth - PDFBase.Container.PageSetup.LeftMargin - PDFBase.Container.PageSetup.LeftMargin;
                    Paragraph.Format.SpaceAfter = 10;

                    if (Image.Width * .4 > MaxWidth)
                    {
                        PDFImage.LockAspectRatio = true;
                        PDFImage.Width = PDFBase.Container.PageSetup.PageWidth - PDFBase.Container.PageSetup.LeftMargin - PDFBase.Container.PageSetup.LeftMargin;
                    }
                    else
                        PDFImage.ScaleWidth = .4;
                }
                catch (Exception Exception)
                {
                    DrawText($"Image Error: {Exception.Message})", null, 10);
                }
            }
            else if (Element is ICheckListAttachment AttachmentElement)
            {
                try
                {
                    var Key = AttachmentElement.GetTarget().Key;
                    if (Key == null && !string.IsNullOrEmpty(Description))
                        DrawText(Description, PDFBase.BoldFont, 5);
                    else if (AttachmentElement.Key != null && AttachmentElement.FileType == "Image")
                    {
                        if (!string.IsNullOrEmpty(Description))
                            DrawText(Description, PDFBase.BoldFont, 5);

                        Paragraph Paragraph = PDFBase.Container.AddParagraph();
                        Paragraph.Format.Alignment = ParagraphAlignment.Center;
                        var StorageFile = await CheckListPDF.StorageService.GetAsync(CheckListPDF.ConfigurationService.InstancesStorageBucket, Key);
                        byte[] Data;
                        Bitmap Image;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await StorageFile.Content.CopyToAsync(ms);
                            Image = new Bitmap(ms);
                            Data = ms.ToArray();
                        }

                        var PDFImage = Paragraph.AddImage(Data);
                        var MaxWidth = PDFBase.Container.PageSetup.PageWidth - PDFBase.Container.PageSetup.LeftMargin - PDFBase.Container.PageSetup.LeftMargin;
                        Paragraph.Format.SpaceAfter = 10;

                        if (Image.Width * .4 > MaxWidth)
                        {
                            PDFImage.LockAspectRatio = true;
                            PDFImage.Width = PDFBase.Container.PageSetup.PageWidth - PDFBase.Container.PageSetup.LeftMargin - PDFBase.Container.PageSetup.LeftMargin;
                        }
                        else
                            PDFImage.ScaleWidth = .4;
                    }
                    else if (Key != null)
                    {
                        var Index = await CheckListPDF.AddAttachmentAsync(CheckListPDF.ConfigurationService.InstancesStorageBucket, Key, AttachmentElement);

                        if (!string.IsNullOrEmpty(Description))
                            DrawText(Description, PDFBase.BoldFont, 5);

                        DrawText($"(see attachment {Index})", null, 10);
                    }
                }
                catch (Exception Exception)
                {
                    DrawText($"Attachment Error: {Exception.Message})", null, 10);
                }
            }
        }
    }
}