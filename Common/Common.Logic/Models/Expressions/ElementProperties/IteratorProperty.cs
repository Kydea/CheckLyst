using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class IteratorProperty : ElementProperty
    {
        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListGroup Group)) throw new Exception($"The element with code {Code} is not a Group");
            if (!Group.Iterator) throw new Exception($"The element with code {Code} is not an Iterator");
            DataType = DataType.Number;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var Iterator = (ICheckListGroup)ElementsBranch.Elements[Code];
            return new TypedValue(DataType.Number, (decimal)Iterator.Iterations.Where(x => !x.Removed).Count());
        }

        protected override DependencyRelation.Properties DependencyProperty => DependencyRelation.Properties.Iterations;

        public override string ToString()
        {
            return $"{Code}.Count";
        }
    }
}
