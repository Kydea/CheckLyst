using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class Each : IteratorFunction
    {
        public Each()
        {

        }

        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            base.Check(Prototype, IteratorStack);
            InnerDataType = DataType;
            DataType = DataType.Array;
        }

        protected override TypedValue GetValue(List<TypedValue> Values)
        {
            return new TypedValue(DataType.Array, Values);
        }

        public override string ToString()
        {
            return $"{IteratorCode}.Each({Argument?.ToString() ?? "#"})"; 
        }
    }
}
