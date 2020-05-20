using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Field : Item, ICheckListField
	{
		public override string TypeName => "Field";

		public string ExternalSourceColumn { get; set; }
		public string ExternalSourceName { get; set; }

        public int LinesCount { get; set; }
		public string Value { get; set; }
        public Formula ValueFormula { get; } = new Formula();
		public string ValueFormulaString { get; set; }

        private List<string> _ValuesList;
        public List<string> ValuesList { get { return _ValuesList; } set { _ValuesList = value == null || !value.Any() ? null : value; } }

        [JsonIgnore]
        public List<FieldRecord> History { get; } = new List<FieldRecord>();
        [JsonIgnore]
        public string ModifiedBy { get; }
        [JsonIgnore]
        public DateTimeOffset? ModifiedOn { get; }

        [JsonIgnore]
        public DataType ValueType { get; private set; } = DataType.Text;
        public string ValueTypeName
        {
            get { return ValueType.TypeName; }
            set { ValueType = DataType.FromName(value); }
        }

        public string DefaultValue { get; set; }

        public bool LocalTimeZone { get; set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var field = new Field
            {
                Group = parent,
                LinesCount = LinesCount,
                Value = Value,
                ValueFormulaString = ValueFormulaString,
                ValueTypeName = ValueTypeName,
                DefaultValue = DefaultValue,
                LocalTimeZone = LocalTimeZone,
                ValuesList = ValuesList,
                ExternalSourceName = ExternalSourceName,
                ExternalSourceColumn = ExternalSourceColumn
            };
            Copy(this, field);
            return field;
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();

            ValueFormula.BuildDependencies(ValueFormulaString, null);
            foreach (var Key in ValueFormula.ValueKeys)
                if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                    AddDependingRelation(DependencyRelation.Properties.Value, Expression, DependencyRelation.Properties.Value);

            if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Value);

            if (ExternalSourceName != null)
            {
                if (!CheckList.ExternalSourceDependencyNodes.TryGetValue(ExternalSourceName, out var ExternalSourceNode))
                    throw new Exception($"The property value references to the inexistent external source {ExternalSourceName} for {this}");

                AddDependingRelation(DependencyRelation.Properties.Value, CheckList.ExternalSourceDependencyNodes[ExternalSourceName], DependencyRelation.Properties.Value);
            }
        }

    }
}
