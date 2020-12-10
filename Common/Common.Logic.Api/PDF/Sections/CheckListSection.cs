using System;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class CheckListSection : PDFSection
    {
        public ICheckListElement Element;

        public CheckListSection(CheckListPDF CheckListPDF, ICheckListElement Element, int Indentation) : base(CheckListPDF, Indentation)
        {
            this.CheckListPDF = CheckListPDF;
            this.Element = Element;
        }

        public CheckListPDF CheckListPDF { get; }

        public override Task RenderAsync()
        {
            if (Element.PageBreak) PDFBase.Container.AddPageBreak();
            return Task.CompletedTask;
        }

        public virtual Task RenderChildrenAsync() => Task.CompletedTask;
    }
}