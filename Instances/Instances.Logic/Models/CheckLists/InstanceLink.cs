using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using System.Linq;
using CheckListManager.Instances.Logic.Exceptions;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Data;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class InstanceLink : Item, ICheckListInstanceLink
    {
		public override string TypeName => "InstanceLink";
		public InstanceLinkActions Actions { get; set; }
		public string PrototypeName { get; set; }
        public List<InstanceLinkRecord> History { get; } = new List<InstanceLinkRecord>();
        public string ModifiedBy { get; private set; }
        public DateTimeOffset? ModifiedOn { get; private set; }
        public InstanceLinkRecord Record { get; set; }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            ItemModel.Actions = Actions;

            if (History.Any())
                ItemModel.History = History.Select(x => new {
                    x.ID,
                    x.Code,
                    x.Description,
                    x.State,
                    x.CreateBy,
                    x.CreateOn
                });
            return ItemModel;
        }

        public override bool IsValidByDefault() => Record.State == InstanceState.Confirmed;

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Value);
        }

        protected override void OnUpdate(DependencyRelation.Properties Property)
        {
            switch (Property)
            {
                case DependencyRelation.Properties.Value:
                    break;
                default:
                    base.OnUpdate(Property);
                    break;
            }
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
		{
			var instanceLink = new InstanceLink
			{
				Group = parent,
                PrototypeName = PrototypeName
			};

			Copy(this, instanceLink);
			return instanceLink;
		}
    }
}
