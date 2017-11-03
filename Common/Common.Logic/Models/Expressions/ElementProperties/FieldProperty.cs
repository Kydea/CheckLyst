using System;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class FieldProperty : ElementProperty
    {
        public FieldProperty(string code) : base(code)
        {
        }

        protected override void Check(ICheckListElement element)
        {
            if (element is ICheckListField field)
				DataType = field.ValueType;
			else
				throw new Exception("The element with code " + Code + " is not an Field");
		}

        protected override TypedValue GetValue(ICheckListElement element)
        {
	        throw new NotImplementedException();
	        //var field = (ICheckListField)element;
            //return new TypedValue(field.ValueType, field.GetValue());
		}
	}
}
