using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class InstanceLinkProperty : ElementProperty
    {
        private const string INSTANCE_STATE = "InstanceState", INSTANCE_DESCRIPTION = "InstanceDescription", INSTANCE_ID = "InstanceID", INSTANCE_CODE = "InstanceCode";

        public InstanceLinkProperty(string Property)
        {
            this.Property = Property;
        }

        protected override DependencyRelation.Properties GetDependencyProperty(DependencyNode DependingElement) => DependencyRelation.Properties.Value;

        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListInstanceLink)) throw new Exception($"The element with code {Code} is not an Instance Link");
            DataType =
                Property == INSTANCE_STATE ? DataType.Text :
                Property == INSTANCE_DESCRIPTION ? DataType.Text :
                Property == INSTANCE_ID ? DataType.Number :
                Property == INSTANCE_CODE ? DataType.Text :
                throw new ArgumentException();
        }

        protected override TypedValue GetValue(ICheckListElement Element, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            var InstanceLink = (ICheckListInstanceLink)Element;
            return
                Property == INSTANCE_STATE ? new TypedValue(DataType.Text, InstanceLink.Record.State) :
                Property == INSTANCE_DESCRIPTION ? new TypedValue(DataType.Text, InstanceLink.Record.Description) :
                Property == INSTANCE_ID ? new TypedValue(DataType.Number, (decimal?)InstanceLink.Record.ID) :
                Property == INSTANCE_CODE ? new TypedValue(DataType.Text , InstanceLink.Record.ID.HasValue ? InstanceLink.Record.Code : null) :
                throw new ArgumentException();
        }
    }
}
