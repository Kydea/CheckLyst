using System.Threading.Tasks;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Page : CheckListSection
    {
        public Page(CheckListPDF PDFBase, ICheckListPage Page, int Indentation = 1) : base(PDFBase, Page, Indentation)
        {}

        public override async Task RenderAsync()
        {
            await base.RenderAsync();
            var PageElement = (ICheckListPage)Element;

            DrawText(PageElement.Description, PDFBase.TitleFont1, 20);
            await RenderChildrenAsync();
        }

        public override async Task RenderChildrenAsync()
        {
            foreach (var Group in ((ICheckListPage)Element).Groups)
                if (Group.ShowAsTable)
                    await new Table((CheckListPDF)PDFBase, Group, 2).RenderAsync();
                else
                    await new Group((CheckListPDF)PDFBase, Group, 2).RenderAsync();
        }
    }
}