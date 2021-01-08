using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class IteratorProperty : ElementProperty
    {
        private readonly string[] Properties = { "Count", "IterationIndex" };

        public IteratorProperty(string Property)
        {
            this.Property = Property;
        }

        protected override void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (!(Element is ICheckListGroup Group)) throw new ParseException($"The element with code {Code} is not a Group", Position);
            if (!Group.Iterator) throw new ParseException($"The element with code {Code} is not an Iterator", Position);
            if (Property == "IterationIndex" && !IteratorStack.Contains(Code)) throw new ParseException($"The group {Code} is not considered in the expression", Position);

            DataType = DataType.Number;
        }

        protected override TypedValue GetValue(ICheckListElement Element, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            var Iterator = (ICheckListGroup)Element;
            return
                Property == "Count" ? new TypedValue(DataType.Number, (decimal)Iterator.Iterations.Where(x => !x.Removed).Count()) :
                Property == "IterationIndex" ? new TypedValue(DataType.Number, (decimal)IterationIndexes[Iterator.Code]) :
                throw new ArgumentException();
        }

        protected override DependencyRelation.Properties GetDependencyProperty(DependencyNode DependingElement) => DependencyRelation.Properties.Iterations;

        public override string ToString() => $"{Code}.{Property}";
    }
}
