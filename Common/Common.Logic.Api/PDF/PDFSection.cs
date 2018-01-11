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

        //int RoundUp(int NumToRound, int Multiple)
        //{
        //    if (Multiple == 0)
        //        return NumToRound;

        //    int Remainder = NumToRound % Multiple;
        //    if (Remainder == 0)
        //        return NumToRound;

        //    return NumToRound + Multiple - Remainder;
        //}

        //public class TextSize
        //{
        //    public static readonly TextSize Empty = new TextSize(0, 0, 0, 0, 0, 0);
        //    public float Width, Height, LineLength, LineHeight, CharWidth;
        //    public int LineCount;
        //    public TextSize(float Width, float Height, int LineCount, float CharWidth, float LineLength, float LineHeight)
        //    {
        //        this.Width = Width;
        //        this.Height = Height;
        //        this.LineCount = LineCount;
        //        this.LineHeight = LineHeight;
        //        this.LineLength = LineLength;
        //        this.CharWidth = CharWidth;
        //    }
        //}

        //public virtual TextSize MeasureString(string Text, System.Drawing.Font Font, float MaxWidth = 0)
        //{
        //    float Width = BaseFont.GetWidthPoint(Text, Font.Size);
        //    float Height =
        //        RoundUp((int)(BaseFont.GetAscentPoint(Text, Font.SizeInPoints) - BaseFont.GetDescentPoint(Text, Font.SizeInPoints)) - 5, 10);

        //    float LineHeight = Height + 10;

        //    int LineCount = 1;
        //    if (MaxWidth > 0 && Width > MaxWidth)
        //    {
        //        LineCount = 2;
        //        while ((Width = Width - MaxWidth) > MaxWidth)
        //            LineCount++;

        //        Height = (Height + 10) * LineCount;
        //        Width = MaxWidth;
        //    }

        //    float CharWidth = (float)(BaseFont.GetWidthPoint(Text, Font.Size) / Text.Length + (LineCount == 1 ? 0 : 0.5f));
        //    float LineLength = Width / CharWidth;

        //    return new TextSize(Width, Height, LineCount, CharWidth, LineLength, LineHeight);
        //}

        //public virtual void DrawString(string Text, System.Drawing.Font Font, Color Color, RectangleF Rectangle, StringFormat Format, bool DontOverflow = false)
        //{
        //    if (Text != null)
        //    {
        //        if (DontOverflow)
        //        {
        //            var LineLength = MeasureString(Text, Font, Rectangle.Width).LineLength;
        //            Text = Text.Substring(0, Text.Length > LineLength ? (int)LineLength : Text.Length);
        //        }
        //        PDFBase.PdfContentByte.BeginText();
        //        PDFBase.PdfContentByte.SetFontAndSize(BaseFont, Font.Size);
        //        PDFBase.PdfContentByte.SetTextMatrix(Rectangle.Left, PDFBase.Document.PageSize.Height - Rectangle.Top - 10);
        //        PDFBase.PdfContentByte.ShowText(Text);
        //        PDFBase.PdfContentByte.EndText();
        //    }
        //}

        //public virtual void DrawString(string Text, System.Drawing.Font Font, Color Color, RectangleF Rectangle, StringFormat Format, TextSize Size)
        //{
        //    if (Text != null)
        //    {
        //        for (int i = 0; i < Size.LineCount; i++)
        //        {
        //            int Start = (int)(i * Size.LineLength);
        //            int Length = ((i+1) * Size.LineLength < Text.Length) ? (int)Size.LineLength : (int)(Text.Length - i * Size.LineLength);
        //            string Line = Text.Substring(Start, Length);
        //            CheckAndDrawString(Line, Font, Color);

        //            if(i == Size.LineCount-1)
        //            //Check for remaining text
        //            {
        //                if (Text.Length - Start > Size.LineLength)
        //                    Size.LineCount++;
        //            }

        //            if(i != Size.LineCount-1)
        //                PDFBase.CurrentRenderingY += Size.LineHeight;
        //        }
        //    }
        //}

        //private static iTextSharp.text.Font GetFont(BaseFont BaseFont, System.Drawing.Font Font, Color Color)
        //{
        //    return new iTextSharp.text.Font(BaseFont, Font.Size, (Font.Bold ? iTextSharp.text.Font.BOLD : 0) + (Font.Italic ? iTextSharp.text.Font.ITALIC : 0), new BaseColor(Color));
        //}

        //private static int GetHorizontalAlignment(RectangleF Rectangle, StringFormat Format)
        //{
        //    switch (Format.Alignment)
        //    {
        //        case StringAlignment.Far:
        //            return Element.ALIGN_RIGHT;
        //        case StringAlignment.Center:
        //            return Element.ALIGN_CENTER;
        //        default:
        //            return Element.ALIGN_LEFT;
        //    }
        //}

        //private static RectangleF GetVerticalAlignment(RectangleF Rectangle, StringFormat Format, float SimulatedHeight, System.Drawing.Font Font)
        //{
        //    switch (Format.LineAlignment)
        //    {
        //        case StringAlignment.Far:
        //            Rectangle.Y += Rectangle.Height - SimulatedHeight;
        //            Rectangle.Height -= Rectangle.Height - SimulatedHeight;
        //            break;
        //        case StringAlignment.Center:
        //            Rectangle.Y += (Rectangle.Height - SimulatedHeight - Font.Height / 2) / 2;
        //            Rectangle.Height -= (Rectangle.Height - SimulatedHeight - Font.Height / 2) / 2;
        //            break;
        //        case StringAlignment.Near:
        //            Rectangle.Y -= Font.Height / 4;
        //            Rectangle.Height += Font.Height / 4;
        //            break;
        //    }

        //    return Rectangle;
        //}

        //private static PointF GetTextPosition(RectangleF Rectangle, SizeF Size, StringFormat Format)
        //{
        //    float X = 0;
        //    switch (Format.Alignment)
        //    {
        //        case StringAlignment.Far:
        //            X = Rectangle.X + Rectangle.Width - Size.Width;
        //            break;
        //        case StringAlignment.Center:
        //            X = Rectangle.X + (Rectangle.Width - Size.Width) / 2;
        //            break;
        //        case StringAlignment.Near:
        //            X = Rectangle.X;
        //            break;
        //    }

        //    float Y = 0;
        //    switch (Format.LineAlignment)
        //    {
        //        case StringAlignment.Far:
        //            Y = Rectangle.Y + Rectangle.Height - Size.Height;
        //            break;
        //        case StringAlignment.Center:
        //            Y = Rectangle.Y + Rectangle.Height / 2;
        //            break;
        //        case StringAlignment.Near:
        //            Y = Rectangle.Y + Size.Height;
        //            break;
        //    }

        //    return new PointF(X, Y);
        //}

        //public virtual void FillRectangle(Color Color, System.Drawing.Rectangle Rectangle)
        //{
        //    PDFBase.PdfContentByte.SetColorFill(new BaseColor(Color));
        //    PDFBase.PdfContentByte.Rectangle(Rectangle.X, PDFBase.Document.PageSize.Height - Rectangle.Height - Rectangle.Y, Rectangle.Width, Rectangle.Height);
        //    PDFBase.PdfContentByte.Fill();
        //}

        //public virtual void DrawRectangle(Pen Pen, System.Drawing.Rectangle Rectangle)
        //{
        //    PDFBase.PdfContentByte.SetColorStroke(new BaseColor(Pen.Color));
        //    PDFBase.PdfContentByte.SetLineWidth(Pen.Width);
        //    PDFBase.PdfContentByte.Rectangle(Rectangle.X, PDFBase.Document.PageSize.Height - Rectangle.Height - Rectangle.Y, Rectangle.Width, Rectangle.Height);
        //    PDFBase.PdfContentByte.Stroke();
        //}

        //public virtual void DrawLine(Pen Pen, float X1, float Y1, float X2, float Y2)
        //{
        //    PDFBase.PdfContentByte.SetColorStroke(new BaseColor(Pen.Color));
        //    PDFBase.PdfContentByte.SetLineWidth(Pen.Width);
        //    PDFBase.PdfContentByte.MoveTo(X1, PDFBase.Document.PageSize.Height - Y1);
        //    PDFBase.PdfContentByte.LineTo(X2, PDFBase.Document.PageSize.Height - Y2);
        //    PDFBase.PdfContentByte.Stroke();
        //}

        //public virtual void DrawImage(System.Drawing.Image Image, float X, float Y, float Width, float Height)
        //{
        //    //throw new NotImplementedException();
        //}

        //public virtual TextSize DrawMeasuredString(String Text, System.Drawing.Font Font, Color Color, Point Position)
        //{
        //    var Size = MeasureString(Text, Font);
        //    DrawString(Text, Font, Color, new RectangleF(Position.X, Position.Y, Size.Width + 5, Size.Height + 10), StringFormat.GenericDefault);
        //    return Size;
        //}

        //public virtual TextSize DrawTextField(String Label, System.Drawing.Font Font, Color Color, Point Position, Pen Pen, Size Field, float MinWidth = 100)
        //{
        //    var Size = DrawMeasuredString(Label, Font, Color, Position);
        //    Size.Width = MinWidth > Size.Width ? MinWidth : Size.Width;
        //    DrawRectangle(Pen, new System.Drawing.Rectangle(Position.X, (int)(Position.Y + Size.Height + 10), Field.Width, Field.Height));
        //    return Size;
        //}

        //public virtual TextSize DrawTextField(String Label, System.Drawing.Font Font, Color Color, float PositionX, float PositionY, Pen Pen, Size Field, float MinWidth = 100)
        //{
        //    return DrawTextField(Label, Font, Color, new Point((int)PositionX, (int)PositionY), Pen, Field, MinWidth);
        //}

        //public virtual TextSize DrawDynamicTextField(String Label, System.Drawing.Font Font, Color Color, float PositionX, float PositionY, Pen Pen, float MinWidth = 100)
        //{
        //    var Size = DrawMeasuredString(Label, Font, Color, new Point((int)PositionX, (int)PositionY));
        //    Size.Width = MinWidth > Size.Width ? MinWidth : Size.Width;
        //    DrawRectangle(Pen, new System.Drawing.Rectangle((int)PositionX, (int)(PositionY + Size.Height + 10), (int)Size.Width, 20));
        //    return Size;
        //}

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
