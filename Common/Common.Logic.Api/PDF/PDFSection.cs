using MigraDoc.DocumentObjectModel;
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

        public abstract Task RenderAsync(params object[] State);

        public Paragraph DrawText(string Text, Font Font = null, int? SpaceAfter = null)
        {
            Paragraph Paragraph = PDFBase.Container.AddParagraph();
            Paragraph.SetFont(Font ?? PDFBase.GenericFont);
            Paragraph.AddText(Text ?? string.Empty);
            if (SpaceAfter.HasValue) Paragraph.Format.SpaceAfter = SpaceAfter.Value;
            return Paragraph;
        }
    }
}
