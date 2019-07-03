using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Iteration : Group, ICheckListIteration
    {
        public override string TypeName => "Iteration";
        public int Index { get; set; }

        private void AddAvailableSubItem(ICheckListGroup Group)
        {
            if (!Group.Iterator)
                Group.Items.ForEach(Item =>
                {
                    if (!string.IsNullOrEmpty(Item.Code)) ElementsList.Add(Item);
                    if (Item is ICheckListGroup group) AddAvailableSubItem(group);
                });
        }

        public override void Setup()
        {
            //loads or setup for the first time the iteration
            var Loading = Id > 0;
            CheckList.AddItem(this);
            if (Loading)
            {
                IterationItem = true;
                foreach (var Item in Items)
                {
                    Item.Iteration = this;
                    Item.IterationItem = true;
                }
            }
            else
                foreach (var Item in Group.Items)
                {
                    var Clone = Item.Clone(this);
                    Clone.Iteration = this;
                    Items.Add(Clone);
                }
            SetupItems();

            ElementsList = new List<ICheckListElement>();
            AddAvailableSubItem(this);
        }

        public List<ICheckListElement> ElementsList { get; private set; }

        public Dictionary<string, ICheckListElement> Elements
        {
            get
            {
                var Result = new Dictionary<string, ICheckListElement>(ElementsList.Count);
                ElementsList.ForEach(x => Result.Add(x.Code, x));
                var Parent = Group;
                while (Parent != null)
                    if (!Parent.IterationItem)
                        Parent = null;
                    else
                    {
                        if (Parent is Iteration Iteration)
                            Iteration.ElementsList.ForEach(x => Result.Add(x.Code, x));
                        Parent = Parent.Group;
                    }

                ((Instance)CheckList).ElementsList.ForEach(x => Result.Add(x.Code, x));
                return Result;
            }
        }
    }
}
