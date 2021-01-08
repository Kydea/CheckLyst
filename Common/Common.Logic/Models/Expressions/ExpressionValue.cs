using CheckListManager.Common.Logic.Dependency;
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

        public override void BuildDependencies(ICheckList CheckList, Expression Expression)
        {
            if (!CheckList.Expressions.TryGetValue(ExpressionCode, out var DependingExpression)) throw new ParseException("Expression not found: " + ExpressionCode, Position);
            Expression.AddDependingRelation(DependencyRelation.Properties.Value, DependingExpression, DependencyRelation.Properties.Value);
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            DataType = CheckList.Expressions[ExpressionCode].DataType;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            return ElementsBranch.CheckList.Expressions[ExpressionCode].Value;
        }

        public override string ToString()
        {
            return $"{ExpressionCode}";
        }
    }
}
