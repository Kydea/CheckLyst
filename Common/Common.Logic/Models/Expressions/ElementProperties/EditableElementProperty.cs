using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class EditableElementProperty : ElementProperty
    {
        public const string MODIFIED_BY = "property ModifiedBy", MODIFIED_ON = "property ModifiedOn";

        public EditableElementProperty(string Property)
        {
            this.Property = Property;
        }

        protected override DependencyRelation.Properties GetDependencyProperty(DependencyNode DependingElement) =>
            DependingElement is ICheckListGroup ? DependencyRelation.Properties.Iterations :
            DependingElement is ICheckListSignature ? DependencyRelation.Properties.Signature :
            DependingElement is ICheckListAttachment ? DependencyRelation.Properties.Validity :
            DependencyRelation.Properties.Value;

        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListEditableItem)) throw new Exception($"The element with code {Code} is editable");
            if (Element is ICheckListGroup Group && !Group.Iterator) throw new Exception($"The group {Code} is not an iterator");
            DataType = 
                Property == MODIFIED_BY ? DataType.Text : 
                Property == MODIFIED_ON ? DataType.DateTime : 
                throw new ArgumentException();
        }

        protected override TypedValue GetValue(ICheckListElement Element, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            var EditableItem = (ICheckListEditableItem)Element;
            return
                Property == MODIFIED_BY ? new TypedValue(DataType.Text, EditableItem.ModifiedBy) :
                Property == MODIFIED_ON ? new TypedValue(DataType.DateTime, EditableItem.ModifiedOn) :
                throw new ArgumentException();
        }
    }
}
