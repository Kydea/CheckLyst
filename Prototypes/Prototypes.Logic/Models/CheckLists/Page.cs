using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;
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
                group.Traverse(condition, action);
        }

        public async Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            foreach (var group in Groups)
                await group.TraverseAsync(condition, action);
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            foreach (Group Group in Groups)
                Group.BuildDependencies();
        }

        public override void Check()
        {
            base.Check();
            foreach (Group Group in Groups)
                Group.Check();
        }

        public void CreateInstanceIterations() { throw new NotImplementedException(); }

        public override DependencyNodeKey DependencyNodeKey => base.DependencyNodeKey ?? new DependencyNodeKey("page", Description);
        protected override IEnumerable<DependencyNodeKey> GetDependingNodes()
        {
            foreach (var DependencyNodeKey in base.GetDependingNodes())
                yield return DependencyNodeKey;
            foreach (Group Group in Groups)
                yield return Group.DependencyNodeKey;
        }
    }
}
