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
		public long? PrototypeID { get; set; }
        public List<InstanceLinkRecord> History { get; } = new List<InstanceLinkRecord>();
        public string ModifiedBy { get; private set; }
        public DateTimeOffset? ModifiedOn { get; private set; }
        public long? AttachedInstanceID { get; set; }
        public string AttachedInstanceCode { get; set; }
        public InstanceLinkRecord Record { get; set; }
        public List<InstanceLinkParameter> Parameters { get; set; }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            ItemModel.PrototypeName = PrototypeName;
            ItemModel.PrototypeID = PrototypeID;
            ItemModel.AttachedInstanceID = AttachedInstanceID;
            ItemModel.AttachedInstanceCode = AttachedInstanceCode;
            ItemModel.Actions = Actions;
            ItemModel.Record = Record;

            if (History.Any())
                ItemModel.History = History.Select(x => new {
                    x.ID,
                    x.Code,
                    x.Description,
                    x.State,
                    x.User,
                    x.DateTime
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

        private void UpdateValue()
        {
            if (!Record.ID.HasValue)
                Record = new InstanceLinkRecord { Code = $"{PrototypeName}.?.?.?" };
        }

        protected override void OnUpdate(DependencyRelation.Properties Property)
        {
            switch (Property)
            {
                case DependencyRelation.Properties.Value:
                    UpdateValue();
                    break;
                default:
                    base.OnUpdate(Property);
                    break;
            }
        }

        public override void MarkAllAsChanged(HashSet<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            base.MarkAllAsChanged(Roots);
            MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.Value);
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
		{
			var instanceLink = new InstanceLink
			{
				Group = parent,
                PrototypeName = PrototypeName,
                PrototypeID = PrototypeID
            };

			Copy(this, instanceLink);
			return instanceLink;
		}

        public void Attach(long? id, string code, InstanceLinkRecord record)
        {
            AttachedInstanceID = id;
            AttachedInstanceCode = code;
            Record = record;
            History.Add(record);
            ModifiedBy = record.User;
            ModifiedOn = record.DateTime;
        }
    }
}
