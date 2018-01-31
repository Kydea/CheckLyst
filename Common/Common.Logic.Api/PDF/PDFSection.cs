using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class PDFSection
    {
        protected PDFBase PDFBase { get; private set; }

        public int Indentation = 1;

        public PDFSection(PDFBase PDFBase, int Indentation = 0)
        {
            this.PDFBase = PDFBase;
            this.Indentation = Indentation;
        }

        public abstract void Render(params Object[] State);

        #region Drawing Primitives

        public static BaseFont BaseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        public static iTextSharp.text.Font NoteFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        public static iTextSharp.text.Font SmallFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        public static iTextSharp.text.Font SmallBoldFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        public static iTextSharp.text.Font NormalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        public static iTextSharp.text.Font GenericFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        public static iTextSharp.text.Font GenericBoldFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        public static iTextSharp.text.Font GenericFontGray = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.DARK_GRAY);
        public static iTextSharp.text.Font FirstHeaderFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        public static iTextSharp.text.Font SecondHeaderFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

        public void DrawText(String Text, iTextSharp.text.Font Font = null)
        {
            if (Font == null)
                Font = GenericFont;
            Paragraph Paragraph = new Paragraph(new Chunk(Text, Font));
            PDFBase.Document.Add(Paragraph);
        }

        #endregion

        #region Drawing Complex
        //public bool CheckPageMargin(float Width, float Height, float BreakHeight)
        //{
        //    if (PDFBase.CurrentRenderingX + Width >= PDFBase.Document.PageSize.Width)
        //    {
        //        PDFBase.CurrentRenderingX = Indentation * 20;
        //        PDFBase.CurrentRenderingY += BreakHeight;
        //    }

        //    if (PDFBase.CurrentRenderingY + Height >= PDFBase.Document.PageSize.Height - 20)
        //    {
        //        //if (PDFBase.Document.PageNumber > 0)
        //        //{
        //        //    DrawRectangle(Pens.Black, new System.Drawing.Rectangle(20, (int)PDFBase.PageStartY, (int)PDFBase.Document.PageSize.Width - 40, (int)(PDFBase.Document.PageSize.Height - PDFBase.PageStartY)));
        //        //    PDFBase.PageStartY = 0;
        //        //}
        //        PDFBase.Document.NewPage();
        //        PDFBase.CurrentRenderingX = Indentation * 20;
        //        PDFBase.CurrentRenderingY = 20;

        //        return true;
        //    }

        //    return false;
        //}

        //public void CheckAndDrawString(string Text, System.Drawing.Font Font, Color Color)
        //{
        //    var Size = MeasureString(Text, Font);
        //    CheckPageMargin(Size.Width, Size.Height, 20);
        //    DrawString(Text, Font, Color.Black, new RectangleF(new Point((int)PDFBase.CurrentRenderingX, (int)PDFBase.CurrentRenderingY), SizeF.Empty), StringFormat.GenericDefault);
        //}

        //public TextSize CheckAndDrawMultilineString(string Text, System.Drawing.Font Font, Color Color, float MaxWidth)
        //{
        //    var Size = MeasureString(Text, Font, MaxWidth);
        //    DrawString(Text, Font, Color.Black, new RectangleF(new Point((int)PDFBase.CurrentRenderingX, (int)PDFBase.CurrentRenderingY), SizeF.Empty), StringFormat.GenericDefault, Size);
        //    return Size;
        //}

        #endregion
    }
}
