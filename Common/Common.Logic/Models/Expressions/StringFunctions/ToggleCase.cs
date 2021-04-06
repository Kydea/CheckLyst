using System;
using System.Collections.Generic;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.StringFunctions
{
    public class ToggleCase : UnaryOperator
    {
        private enum Cases { Upper, Lower }
        private Cases Case { get;  }
        private string FunctionName { get; }

        public ToggleCase(string FunctionName)
        {
            Case = FunctionName == "stringtouppercase" ? Cases.Upper : FunctionName == "stringtolowercase" ? Cases.Lower : throw new ArgumentException();
            this.FunctionName = Case == Cases.Upper ? "StringToUpperCase" : "StringToLowerCase";
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Argument.DataType != DataType.Text)
                throw new ParseException($"The argument of {FunctionName} must be a text", Position);

            DataType = DataType.Text;
        }

        protected override TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue) =>
            base.Evaluate(ElementsBranch, ArgumentValue) ??
                new TypedValue(DataType.Text, 
                    ArgumentValue.IsNull ? null :
                    Case == Cases.Lower ? ArgumentValue.TextValue.ToLower() :
                    ArgumentValue.TextValue.ToUpper()
                );

        public override string ToString() =>
            $"{FunctionName}({Argument?.ToString() ?? "#"})";
    }
}