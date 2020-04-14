using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class Each : UnaryIteratorFunction
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);
            InnerDataType = Argument.InnerDataType ?? DataType;
            DataType = DataType.Array;
        }

        protected override TypedValue GetValue(List<TypedValue> Values)
        {
            return new TypedValue(DataType.Array, Values);
        }
 
        protected override string Property => "Eech";
    }
}
