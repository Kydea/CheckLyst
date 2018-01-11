using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Drawing;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Item : CheckListSection
    {
        public ICheckListItem Next;
        bool FirstOfGroup = false;
        public Item(CheckListPDF PDFBase, ICheckListItem Item, ICheckListItem Next, bool FirstOfGroup, int Indentation = 1) : base(PDFBase, Item, Indentation)
        {
            this.FirstOfGroup = FirstOfGroup;
            this.Next = Next;
        }

        public string StripTagsRegex(string Source)
        {
            return System.Text.RegularExpressions.Regex.Replace(Source, "<.*?>", string.Empty);
        }

        void DrawSignature(string Description, string User = null, string Date = null, string Time = null, string Signature = null)
        {
            PdfPTable Table = new PdfPTable(4);

            Table.AddCell(new PdfPCell(new Paragraph("Signed by", GenericBoldFont)) { Padding = 10 });
            Table.AddCell(new PdfPCell(new Paragraph("Date", GenericBoldFont)) { Padding = 10 });
            Table.AddCell(new PdfPCell(new Paragraph("Time", GenericBoldFont)) { Padding = 10 });
            Table.AddCell(new PdfPCell(new Paragraph("Signature", GenericBoldFont)) { Padding = 10 });

            var UserDescription =
                Date == null ? " " :
                String.IsNullOrEmpty(Signature) ? "???" : //PDFBase.DbContext.Users.FirstOrDefault(x => x.Code == User)?.Description : //TODO PDF
                Signature;

            Table.AddCell(new PdfPCell(new Paragraph(User, GenericFont)) { Padding = 10 });
            Table.AddCell(new PdfPCell(new Paragraph(Date, GenericFont)) { Padding = 10 });
            Table.AddCell(new PdfPCell(new Paragraph(Time, GenericFont)) { Padding = 10 });
            Table.AddCell(new PdfPCell(new Paragraph(UserDescription, GenericFont)) { Padding = 10 });

            Table.WidthPercentage = 100;

            PDFBase.Document.Add(Table);

            DrawText("\n");
        }

        public override void Render(params Object[] State)
        {
            if (Element is ICheckListGroup)
            {
                var Group = (ICheckListGroup)Element;
                if (Group.ShowAsTable)
                    new PDF.Sections.Table((CheckListPDF)PDFBase, Group, Indentation + 1).Render();
                else
                    new PDF.Sections.Group((CheckListPDF)PDFBase, Group, Indentation + 1).Render();
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
                {
                    foreach (var Sign in SignatureElement.History)
                    {
                        string[] Date = Sign.DateTime.Split(' ');
                        DrawSignature(String.IsNullOrEmpty(Sign.Note) ? "Signed" : "Canceled", Sign.User, Date[0], Date[1], Sign.Signature);
                    }
                }
                else
                    DrawSignature(SignatureElement.Description);
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

                var Value = "????"; // FieldElement.RawValue; //TODO PDF
                if (!String.IsNullOrEmpty(Value))
                    DrawText(this.StripTagsRegex(Value), GenericFontGray);
                else
                    DrawText("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _", GenericFontGray);

                DrawText("\n");

            }

            #endregion
        }
    }
}