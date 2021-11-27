using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Group : CheckListSection
    {

        public Group(CheckListPDF PDFBase, ICheckListGroup Group, int Indentation = 1) : base(PDFBase, Group, Indentation)
        { }

        public override async Task RenderAsync()
        {
            if (Element == null)
                return;

            await base.RenderAsync();

            var GroupElement = (ICheckListGroup)Element;
            var Description = CheckListPDF.GetItemDescription(GroupElement);

            if (!string.IsNullOrEmpty(Description))
                DrawText(Description, Indentation == 1 ? PDFBase.TitleFont1 : Indentation == 2 ? PDFBase.TitleFont2 : PDFBase.TitleFont3, 10);

            if (GroupElement.Items.Count > 0)
                await RenderChildrenAsync();
        }

        public override async Task RenderChildrenAsync()
        {
            var Group = ((ICheckListGroup)Element);
            var CheckList = ((CheckListPDF)PDFBase).Model;

            CheckListPDF.RenderGroup(Group);

            List<ICheckListItem> Items =
                Group.Iterator ?
                    Group.Iterations.Select(x => (ICheckListItem)x).ToList() :
                    Group.Items;

            for (int i = 0; i < Items.Count; i++)
            {
                var Item = Items[i];
                var NextItem = i == Items.Count - 1 ? null : Items[i + 1];
                await new Item((CheckListPDF)PDFBase, Item, NextItem, i == 0, Indentation).RenderAsync();
            }
        }
    }
}