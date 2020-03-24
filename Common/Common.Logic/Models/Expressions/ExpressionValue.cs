using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class ExpressionValue : Symbol
    {
        public string ExpressionCode { get; set; }

        public override void Check(ICheckList Prototype, Expression Expression, List<string> IteratorStack)
        {
            if (!Prototype.Expressions.ContainsKey(ExpressionCode)) throw new ParseException("Expression not found: " + ExpressionCode, Position);
            var DependngExpression = Prototype.Expressions[ExpressionCode];
            Expression.DependingNodes.Add(DependngExpression.DependencyNodeKey);
            DataType = DependngExpression.DataType;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            return ElementsBranch.CheckList.Expressions[ExpressionCode].Value;
        }

        public override string ToString()
        {
            return $"{ExpressionCode}";
        }
    }
}
