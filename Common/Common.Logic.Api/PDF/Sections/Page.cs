using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Page : CheckListSection
    {
        public Page(CheckListPDF PDFBase, ICheckListPage Page, int Indentation = 1) : base(PDFBase, Page, Indentation)
        {}

        public override void Render(params object[] State)
        {
            var PageElement = (ICheckListPage)Element;

            DrawText(PageElement.Description, PDFBase.TitleFont1, 20);
            RenderChildren();
        }

        public override void RenderChildren()
        {
            foreach (var Group in ((ICheckListPage)Element).Groups)
                if (Group.ShowAsTable)
                    new Table((CheckListPDF)PDFBase, Group, 2).Render();
                else
                    new Group((CheckListPDF)PDFBase, Group, 2).Render();
        }
    }
}