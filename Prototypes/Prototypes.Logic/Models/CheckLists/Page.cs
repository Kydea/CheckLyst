using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Page : Element, ICheckListPage
	{
		public string Description { get; set; }
		public List<ICheckListGroup> Groups { get; set; } = new List<ICheckListGroup>();
        public bool IsLocked { get; set; }

        public override void Setup()
        {
            base.Setup();
            foreach (var Group in Groups)
            {
                Group.CheckList = CheckList;
                Group.Setup();
            }
        }
        
        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            foreach (var group in Groups)
            {
                if (condition(group))
                {
                    action(group);
                }

                group.Traverse(condition, action);
            }
        }

        public override void Check()
        {
            base.Check();
            foreach (Group Group in Groups)
            {
                Group.CheckList = CheckList;
                Group.Check();
            }
        }

        public void CreateInstanceIterations() { throw new NotImplementedException(); }
    }
}
