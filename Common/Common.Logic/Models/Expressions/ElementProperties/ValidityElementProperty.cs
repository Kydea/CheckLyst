using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class ValidityElementProperty : ElementProperty
    {
        public const string VALID = "property Valid";

        public ValidityElementProperty()
        {
            Property = VALID;
        }

        protected override DependencyRelation.Properties GetDependencyProperty(DependencyNode DependingElement) => DependencyRelation.Properties.Validity;

        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListValidityElement)) throw new Exception($"The element with code {Code} is a validity element");
            DataType = DataType.Boolean;
        }

        protected override TypedValue GetValue(ICheckListElement Element, Dictionary<string, int> IterationIndexes, string TimeZone)
            => new TypedValue(DataType.Boolean, ((ICheckListValidityElement)Element).Valid);
    }
}
