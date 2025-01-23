using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class InstanceProperty : UnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Argument.DataType != DataType.Text)
                throw new ParseException("The argument of IntanceProperty must be a text", Position);

            if (Argument is Constant constant && !CheckList.InstanceRuntimeValues.ContainsKey(constant.GetValue(null, null, null).TextValue))
                throw new ParseException($"Invalid property", Position);

            DataType = DataType.Text;
        }

        protected override TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue) =>
            ElementsBranch.CheckList.InstanceRuntimeValues.TryGetValue(ArgumentValue.TextValue, out var value) ?
                new TypedValue(DataType.Text, value) :
                new TypedValue(DataType.Error, $"Invalid property {ArgumentValue.TextValue}");

        public override string ToString() =>
            $"InstanceProperty({Argument?.ToString() ?? "#"})";
    }
}
