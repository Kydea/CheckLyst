using System;
using System.Collections.Generic;
using System.Linq;
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

        public override dynamic GetViewModel()
        {
            var ElementModel = base.GetViewModel();
            ElementModel.Description = Description;
            ElementModel.Groups = Groups.Where(g => !g.Hidden).Select(g => ((Group)g).GetViewModel());
            ElementModel.Lock = Lock;
            if (Hidden) ElementModel.Hidden = true;
            return ElementModel;
        }

        public void CreateInstanceIterations()
        {
            foreach (Group Group in Groups)
            {
                Group.CreateInstanceIterations();
                Group.SetupIterationsIndexes();
            }
        }

        public override void Setup()
        {
            base.Setup();

            ICheckListGroup previous = null;
            foreach (Group group in Groups)
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
                if (!group.Valid)
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

        public override void ClearDependencies()
        {
            base.ClearDependencies();
            foreach (Group Group in Groups)
                Group.ClearDependencies();
        }

        public override string DependencyClassName => "page";
        public override int? DependencyObjectID => Index;
        public override string ToString() => $"page {Index + 1}";
    }
}
