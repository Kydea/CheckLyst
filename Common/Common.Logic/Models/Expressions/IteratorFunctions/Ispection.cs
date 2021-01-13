using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public class Ispection : UnaryIteratorFunction
    {
        private readonly string[] Operations = { "Any", "All" };

        public Ispection(string Operation)
        {
            if (!Operation.In(Operation)) throw new ArgumentException($"Invalid operation {Operation}");
            Property = Operation;
        }

        protected override string Property { get; }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);
            if (DataType != DataType.Boolean)
                throw new ParseException($"Operands of function {Property} must be booleans", Position);
            DataType = DataType.Boolean;
        }

        protected override TypedValue GetValue(ICheckListGroup Iterator, List<TypedValue> Values)
        {
            var Result =
                Property == "Any" ? Values.Any(TypedValue => TypedValue.BooleanValue) :
                Property == "All" ? Values.All(TypedValue => TypedValue.BooleanValue) :
                throw new ArgumentException();
            
            return new TypedValue(DataType.Boolean, Result);
        }
    }
}
