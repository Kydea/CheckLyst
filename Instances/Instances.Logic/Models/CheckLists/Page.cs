using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Page : Element, ICheckListPage
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public List<ICheckListGroup> Groups { get; set; } = new List<ICheckListGroup>();

        public Lock Lock { get; set; }

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

        public override bool IsValidByDefault()
        {
            foreach (var group in Groups)
                if (!Valid)
                    return false;
            return true;
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

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            foreach (Group Group in Groups)
            {
                if (HiddenCondition == Conditions.False || HiddenCondition == Conditions.Expression)
                    Group.AddDependingRelation(DependencyRelation.Properties.Hidden, this, DependencyRelation.Properties.Hidden);

                if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                    AddDependingRelation(DependencyRelation.Properties.Validity, Group, DependencyRelation.Properties.Validity);

                Group.BuildDependencies();
            }
        }

        public override void MarkAllAsChanged(List<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            base.MarkAllAsChanged(Roots);
            foreach (Group Group in Groups)
                Group.MarkAllAsChanged(Roots);
        }

        public override string DependencyClassName => "page";
        public override int? DependencyObjectID => Index;
    }
}
