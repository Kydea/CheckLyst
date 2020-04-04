using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Page : Element, ICheckListPage
    {
        private bool _valid;

        public int Index { get; set; }
        public string Description { get; set; }
        public List<ICheckListGroup> Groups { get; set; } = new List<ICheckListGroup>();

        public Lock Lock { get; set; }

        public override bool IsValidByDefault() => _valid;

        public void CreateInstanceIterations()
        {
            foreach (var Group in Groups)
                Group.CreateInstanceIterations();
        }

        public override void Setup()
        {
            base.Setup();

            ICheckListGroup previous = null;
            foreach (var group in Groups)
            {
                group.CheckList = CheckList;
                group.Page = this;
                group.Previous = previous;
                group.Setup();
                previous = group;
            }
        }
        protected override void UpdateValue()
        {
        }

        protected override void UpdateOther()
        {
        }

        protected override void UpdateValid()
        {
			_valid = true;
			foreach (var group in Groups)
				_valid = _valid && group.Valid;

            base.UpdateValid();
        }

        public override void UpdateReadOnly(string userCode, bool ReadOnlyInstance)
		{
            if (Lock?.User != userCode || ReadOnlyInstance)
                ReadOnly = true;
            else
                base.UpdateReadOnly(userCode, ReadOnlyInstance);

            foreach (var group in Groups)
				((Group)group).UpdateReadOnly(userCode, ReadOnlyInstance);
		}

        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            throw new NotImplementedException();
        }

        public Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            throw new NotImplementedException();
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            foreach (Group Group in Groups)
            {
                if (HiddenCondition == Conditions.False || HiddenCondition == Conditions.Expression)
                {
                    Group.DependingNodes.Add(DependencyNodeKey.Clone(DependencyNodeKey.Processes.Hidden, "child"));
                    DependentNodes.Add(Group.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Hidden, "parent"));
                }

                if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                {
                    DependingNodes.Add(Group.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Validity, "parent"));
                    Group.DependentNodes.Add(DependencyNodeKey.Clone(DependencyNodeKey.Processes.Validity, "child"));
                }
                Group.BuildDependencies();
            }
        }

        public override DependencyNodeKey DependencyNodeKey => base.DependencyNodeKey ?? new DependencyNodeKey("page", Index);
    }
}
