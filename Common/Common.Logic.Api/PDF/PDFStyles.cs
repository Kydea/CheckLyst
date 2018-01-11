using System;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public static class PDFStyles
    {

        private volatile static Font
            _SmallFont, _SmallFontBold,
            _NormalFont, _NormalFontBold,
            _MediumFont, _MediumFontBold, _MediumMonospaceBoldFont,
            _BigFontBold, _BigFontRedBold;

        public static Font SmallFont
        {
            get { return _SmallFont ?? (_SmallFont = new Font { Size = 5 }); }
        }

        public static Font SmallFontBold
        {
            get
            {
                if (_SmallFontBold == null)
                {
                    _SmallFontBold = new Font { Size = 5 };
                    _SmallFontBold.SetStyle(Font.BOLD);
                }

                return _SmallFontBold;
            }
        }

        public static Font NormalFont
        {
            get { return _NormalFont ?? (_NormalFont = new Font { Size = 7 }); }
        }

        public static Font NormalFontBold
        {
            get
            {
                if (_NormalFontBold == null)
                {
                    _NormalFontBold = new Font { Size = 7 };
                    _NormalFontBold.SetStyle(Font.BOLD);
                }

                return _NormalFontBold;
            }
        }

        public static Font MediumFont
        {
            get { return _MediumFont ?? (_MediumFont = new Font { Size = 11 }); }
        }

        public static Font MediumFontBold
        {
            get
            {
                if (_MediumFontBold == null)
                {
                    _MediumFontBold = new Font { Size = 11 };
                    _MediumFontBold.SetStyle(Font.BOLD);
                }

                return _MediumFontBold;
            }
        }

        public static Font MediumMonospaceBoldFont
        {
            get { return _MediumMonospaceBoldFont ?? (_MediumMonospaceBoldFont = new Font(BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false), 11, Font.BOLD, Red)); }
        }

        public static Font BigFontBold
        {
            get
            {
                if (_BigFontBold == null)
                {
                    _BigFontBold = new Font { Size = 15 };
                    _BigFontBold.SetStyle(Font.BOLD);
                }

                return _BigFontBold;
            }
        }

        public static Font BigFontRedBold
        {
            get
            {
                if (_BigFontRedBold == null)
                {
                    _BigFontRedBold = new Font { Size = 15 };
                    _BigFontRedBold.SetStyle(Font.BOLD);
                    _BigFontRedBold.SetColor(92, 0, 0);
                }

                return _BigFontRedBold;
            }
        }


        private volatile static BaseColor _Black, _Red;

        public static BaseColor Black
        {
            get { return _Black ?? (_Black = new BaseColor(0, 0, 0)); }
        }

        public static BaseColor Red
        {
            get { return _Red ?? (_Red = new BaseColor(92, 0, 0)); }
        }
    }

    internal static class PDFExtension
    {
        private static float GetWidth(float TotalWidth, int Index)
        {
            return (Index % 2) == 0 ? TotalWidth * .3f : TotalWidth * .2f;
        }

        internal static PdfPTable CreateTable(this Document Document, int Columns, string Caption, BaseColor BorderColor, int Border = 1)
        {
            if (Document == null) throw new ArgumentException("Document must be not null");
            if (Columns == 0) throw new ArgumentException("Columns must be greater then 0");

            var Table = new PdfPTable(Columns + 2) { HorizontalAlignment = Element.ALIGN_LEFT, SpacingBefore = 20, SplitLate = false };
            Table.SetTotalWidth(new float[] { 10 }.Concat(Enumerable.Range(1, Columns).Select(x => GetWidth(Document.PageSize.Width - Document.LeftMargin - Document.RightMargin - 20, x))).Concat(new float[] { 10 }).ToArray());
            Table.LockedWidth = true;

            var Cell = new PdfPCell();
            Cell.SetBorder(PdfPCell.TOP_BORDER + PdfPCell.LEFT_BORDER, BorderColor, Border);
            Table.AddCell(Cell);

            Cell = new PdfPCell { Colspan = Columns };
            Cell.SetBorder(PdfPCell.TOP_BORDER, BorderColor, Border);
            Cell.AddElement(new Paragraph(Caption, PDFStyles.BigFontBold));
            Table.AddCell(Cell);

            Cell = new PdfPCell();
            Cell.SetBorder(PdfPCell.TOP_BORDER + PdfPCell.RIGHT_BORDER, BorderColor, Border);
            Table.AddCell(Cell);

            return Table;
        }

        internal static void CompleteTable(this PdfPTable Table, BaseColor BorderColor, int Border = 1)
        {
            if (Table.Rows.Count > 0)
            {
                var Cell = new PdfPCell { Colspan = Table.NumberOfColumns };
                Cell.FixedHeight = 10;
                Cell.SetBorder(PdfPCell.BOTTOM_BORDER + PdfPCell.LEFT_BORDER + PdfPCell.RIGHT_BORDER, BorderColor, Border);
                Table.AddCell(Cell);
            }
        }

        internal static void SetBorder(this PdfPCell Cell, int Border, BaseColor BorderColor, int BorderWidth)
        {
            Cell.Border = Border;
            Cell.BorderColor = BorderColor;
            Cell.BorderWidth = BorderWidth;
        }

        internal static Paragraph CreateParagraph(this Document Document, string Text, int Alignment, Font Font, int SpacingBefore = 0, int SpacingAfter = 0)
        {
            var Paragraph = new Paragraph(new Chunk(Text) { Font = Font });
            Paragraph.Font = Font;
            Paragraph.Alignment = Alignment;
            Paragraph.SpacingBefore = SpacingBefore;
            Paragraph.SpacingAfter = SpacingAfter;

            return Paragraph;
        }

        public static PdfPCell AddNote(this PdfPCell Cell, string Value)
        {
            var Chunk = new Chunk(Value, PDFSection.NoteFont);
            Chunk.SetTextRise(2f);
            Cell.Phrase.Add(Chunk);
            return Cell;
        }

        public static PdfPCell SetPadding(this PdfPCell Cell, int Value)
        {
            Cell.Padding = Value;
            return Cell;
        }

        public static PdfPCell SetBorder(this PdfPCell Cell, int Value)
        {
            Cell.Border = Value;
            return Cell;
        }

        public static PdfPCell SetHorizontalAlign(this PdfPCell Cell, int Value)
        {
            Cell.HorizontalAlignment = Value;
            return Cell;
        }

        public static PdfPCell SetVerticalAlign(this PdfPCell Cell, int Value)
        {
            Cell.VerticalAlignment = Value;
            return Cell;
        }
    }
}