using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class Count : UnaryIteratorFunction
    {
        protected override TypedValue GetValue(List<TypedValue> Values)
        {
            return new TypedValue(DataType.Number, Values.Count());
        }

        public override string ToString()
        {
            return $"{IteratorCode}.Count({Argument?.ToString() ?? "#"})";
        }
    }
}
