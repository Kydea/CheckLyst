using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.AggregateFunctions
{
    public class Each : AggregateFunction
    {
        public Each()
        {

        }

        public override void Check(ICheckList Prototype)
        {
            base.Check(Prototype);
            DataType = DataType.Array;
        }

        protected override TypedValue GetValue(IEnumerable<TypedValue> Values)
        {
            return new TypedValue(DataType.Array, Values);
        }

        public override string ToString()
        {
            return $"{IteratorCode}.Each({Argument?.ToString() ?? "#"})"; 
        }
    }
}
