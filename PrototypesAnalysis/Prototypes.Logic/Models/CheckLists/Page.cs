using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Page : Element, ICheckListPage
	{
        public Page()
        {
            PageBreak = true;
        }

        public int Index { get; set; }
        public string Description { get; set; }
		public List<ICheckListGroup> Groups { get; set; } = new List<ICheckListGroup>();
        public bool IsLocked { get; set; }

        public override bool Setup()
        {
            var Result = base.Setup();
            foreach (Group Group in Groups)
            {
                Group.CheckList = CheckList;
                Group.PageIndex = Index;
                Result &= Group.Setup();
            }

            return Result;
        }
        
        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            foreach (Group group in Groups)
                group.Traverse(condition, action);
        }

        public async Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            foreach (Group group in Groups)
                await group.TraverseAsync(condition, action);
        }

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();
            foreach (Group Group in Groups)
            {
                if (HiddenCondition != Conditions.True || HiddenCondition == Conditions.Expression)
                    Group.AddDependingRelation(DependencyRelation.Properties.Hidden, this, DependencyRelation.Properties.Hidden);

                if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                    AddDependingRelation(DependencyRelation.Properties.Validity, Group, DependencyRelation.Properties.Validity);

                Result &= Group.BuildDependencies();
            }

            return Result;
        }

        public override bool Check()
        {
            var Result = base.Check();
            foreach (Group Group in Groups)
                Result &= Group.Check();

            return Result;
        }

        public void CreateInstanceIterations() { throw new NotImplementedException(); }

        public override string DependencyClassName => "page";
        public override int? DependencyObjectID => Index;
        public override string ToString() => $"page {Index + 1}";
    }
}
