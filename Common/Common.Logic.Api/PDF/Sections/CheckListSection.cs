using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class CheckListSection : PDFSection
    {
        public ICheckListElement Element;

        public CheckListSection(CheckListPDF PDFBase, ICheckListElement Element, int Indentation) : base(PDFBase, Indentation)
        {
            CheckListPDF = PDFBase;
            this.Element = Element;
        }

        public CheckListPDF CheckListPDF { get; }

        public override void Render(params Object[] State)
        { }

        public virtual void RenderChildren()
        { }
    }
}