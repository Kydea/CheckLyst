using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using System.Drawing;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Group : CheckListSection
    {

        public Group(CheckListPDF PDFBase, ICheckListGroup Group, int Indentation = 1) : base(PDFBase, Group, Indentation)
        { }

        public override void Render(params Object[] State)
        {
            if (Element == null)
                return;

            var GroupElement = (ICheckListGroup)Element;

            PDFBase.CurrentRenderingX = Indentation * 20;

            if (!String.IsNullOrEmpty(GroupElement.Description))
            {
                DrawText(GroupElement.Description, Indentation == 1 ? FirstHeaderFont : SecondHeaderFont);
                DrawText("\n");
            }

            if (GroupElement.Items.Count > 0)
                RenderChildren();

        }

        public override void RenderChildren()
        {
            var Group = ((ICheckListGroup)Element);
            var CheckList = ((CheckListPDF)PDFBase).Model;
            //TODO PDF
            //if (CheckList is Prototype)
            //{
            //    Group.CheckList = CheckList;
            //    Group.AddIterations(Math.Max(Group.PrototypeIterations, 1));
            //}

            List<ICheckListItem> Items =
                Group.Iterator ?
                    Group.Iterations.Select(x => (ICheckListItem)x).ToList() :
                    Group.Items;

            for (int i = 0; i < Items.Count; i++)
            {
                var Item = Items[i];
                var NextItem = i == Items.Count - 1 ? null : Items[i + 1];
                new Item((CheckListPDF)PDFBase, Item, NextItem, i == 0, Indentation).Render();
            }
        }
    }
}