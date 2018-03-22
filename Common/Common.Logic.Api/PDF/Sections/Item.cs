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

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Item : CheckListSection
    {
        private string TimeZone;

        public Item(CheckListPDF PDFBase, ICheckListItem Item, ICheckListItem Next, bool FirstOfGroup, int Indentation = 1) : base(PDFBase, Item, Indentation)
        {
            TimeZone = PDFBase.TimeZone;
        }

        private void DrawSignature(string Description, string User = null, string Date = null, string Time = null, string Signature = null)
        {
            PdfPTable Table = new PdfPTable(4) { WidthPercentage = 100 };

            Table.AddCell(new PdfPCell(new Paragraph().SetText("Signed by").SetFont(GenericBoldFont).Padding(10)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText("Date").SetFont(GenericBoldFont).Padding(10)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText("Time").SetFont(GenericBoldFont).Padding(10)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText("Signature").SetFont(GenericBoldFont).Padding(10)));
            Table.CompleteRow();

            Table.AddCell(new PdfPCell(new Paragraph().SetText(User).Padding(10)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText(Date).Padding(10)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText(Time).Padding(10)));
            Table.AddCell(new PdfPCell(new Paragraph().SetText(Date == null ? " " : Signature).Padding(10)));
            Table.CompleteRow();

            PDFBase.Container.Add(Table);
            DrawText("\n");
        }

        public override void Render(params Object[] State)
        {
            if (Element is ICheckListGroup)
            {
                var Group = (ICheckListGroup)Element;
                if (Group.ShowAsTable)
                    new Table((CheckListPDF)PDFBase, Group, Indentation + 1).Render();
                else
                    new Group((CheckListPDF)PDFBase, Group, Indentation + 1).Render();
            }

            #region Text

            else if (Element is ICheckListText TextElement)
            {
                if (!String.IsNullOrEmpty(TextElement.Description))
                {
                    DrawText(TextElement.Description);
                    DrawText("\n");
                }
            }

            #endregion

            #region Signature

            else if (Element is ICheckListSignature SignatureElement)
            {
                if (!String.IsNullOrEmpty(SignatureElement.Description))
                {
                    DrawText(SignatureElement.Description, GenericBoldFont);
                    DrawText("\n");
                }

                if (SignatureElement.History.Count > 0)
                    foreach (var Sign in SignatureElement.History)
                    {
                        var Date = Sign.DateTime.ToLocalTime(TimeZone);
                        DrawSignature(String.IsNullOrEmpty(Sign.Note) ? "Signed by" : "Canceled by", Sign.User, Date.ToString("yyyy-MM-dd"), Date.ToString("HH:mm"), Sign.Signature);
                    }
                else
                    DrawSignature("Signed by");
            }

            #endregion

            #region Field

            else if (Element is ICheckListField FieldElement)
            {
                var Description = FieldElement.Description;

                if (String.IsNullOrEmpty(Description))
                    Description = "(no description)";
                else
                    Description = FieldElement.Description;

                DrawText(Description + "\n", GenericBoldFont);

                DrawText(CheckListPDF.GetFieldValue(this) + "\n", GenericFontGray);
            }

            #endregion
        }
    }
}