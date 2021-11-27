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

        public Page()
        {
            PageBreak = true;
        }

        public int Index { get; set; }
        public string Description { get; set; }
        public List<ICheckListGroup> Groups { get; set; } = new List<ICheckListGroup>();

        public Lock Lock { get; set; }

        public override dynamic GetViewModel(bool ReportModel)
        {
            var ElementModel = base.GetViewModel(ReportModel);
            ElementModel.Description = Description;
            if (Hidden) ElementModel.Hidden = true;
            if (!ReportModel)
            {
                ElementModel.Groups = Groups.Select(g => ((Group)g).GetViewModel(ReportModel));
                ElementModel.Lock = Lock;
            }
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

        public override bool Setup()
        {
            var Result = base.Setup();

            ICheckListGroup previous = null;
            foreach (Group group in Groups)
            {
                group.CheckList = CheckList;
                group.Page = this;
                group.Previous = previous;
                group.Setup();
                previous = group;
            }

            return Result;
        }

        public override bool IsValidByDefault()
        {
            foreach (var group in Groups)
                if (!group.Valid)
                    return false;
            return true;
        }

        public void UpdateReadOnly(string userCode, bool ReadOnlyInstance, bool ignoreNullLock)
		{
            var PageUnlocked = ignoreNullLock && Lock == null || Lock?.User == userCode;
            if (PageUnlocked && !ReadOnlyInstance)
                base.UpdateReadOnly(userCode, false);
            else
                ReadOnly = true;

            foreach (var group in Groups)
				((Group)group).UpdateReadOnly(userCode, ReadOnlyInstance);
		}

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            foreach (Group Group in Groups)
            {
                if (HiddenCondition != Conditions.True || HiddenCondition == Conditions.Expression)
                    Group.AddDependingRelation(DependencyRelation.Properties.Hidden, this, DependencyRelation.Properties.Hidden);

                if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                    AddDependingRelation(DependencyRelation.Properties.Validity, Group, DependencyRelation.Properties.Validity);

                Group.BuildDependencies();
            }
        }

        public override void MarkAllAsChanged(HashSet<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
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
