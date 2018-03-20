using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static Font NoteFont = FontHelper.GetInstance("Helvetica", 7, FontStyle.Regular, Colors.Black);
        public static Font SmallFont = FontHelper.GetInstance("Helvetica", 8, FontStyle.Regular, Colors.Black);
        public static Font SmallBoldFont = FontHelper.GetInstance("Helvetica", 8, FontStyle.Bold, Colors.Black);
        public static Font NormalFont = FontHelper.GetInstance("Helvetica", 9, FontStyle.Regular, Colors.Black);
        public static Font GenericFont = FontHelper.GetInstance("Helvetica", 10, FontStyle.Regular, Colors.Black);
        public static Font GenericBoldFont = FontHelper.GetInstance("Helvetica", 10, FontStyle.Bold, Colors.Black);
        public static Font GenericFontGray = FontHelper.GetInstance("Helvetica", 10, FontStyle.Regular, Colors.DarkGray);
        public static Font FirstHeaderFont = FontHelper.GetInstance("Helvetica", 14, FontStyle.Bold, Colors.Black);
        public static Font SecondHeaderFont = FontHelper.GetInstance("Helvetica", 10, FontStyle.Bold, Colors.Black);

        public void DrawText(String Text, Font Font = null)
        {
            if (Font == null)
                Font = GenericFont;

            if (Text == null)
                Text = String.Empty;

            Paragraph Paragraph = PDFBase.Container.AddParagraph();
            Paragraph.SetFont(Font);
            Paragraph.AddText(Text);
        }
        #endregion
    }
}
