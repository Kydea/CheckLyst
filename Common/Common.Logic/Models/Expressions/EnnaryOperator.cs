using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class EnnaryOperator : Symbol
    {
        public List<Symbol> Arguments { get; } = new List<Symbol>();

        public override Expression Expression
        {
            set
            {
                base.Expression = value;
                Arguments.ForEach(a => a.Expression = value);
            }
        }

        public override void BuildDependencies(ICheckList CheckList, Expression Expression)
        {
            Arguments.ForEach(a => a.BuildDependencies(CheckList, Expression));
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            Arguments.ForEach(a => a.Check(CheckList, Expression, IteratorStack));
            DataType = GetDataType();
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            List<TypedValue> ArgumentValues = Arguments.Select(a => a.GetValue(ElementsBranch, IterationIndexes, TimeZone)).ToList();
            return GetValue(ArgumentValues, IterationIndexes, TimeZone);
        }

        public virtual TypedValue GetValue(List<TypedValue> ArgumentValues, Dictionary<string, int> IterationIndexes, string TimeZone) =>
            ArgumentValues.FirstOrDefault(v => v.Type == DataType.Error) ??
            ArgumentValues.FirstOrDefault(v => v.IsNull);

        protected abstract DataType GetDataType();
        protected abstract string OperatorName { get; }

        public override string ToString()
        {
            return $"OperatorName({string.Join(", ", Arguments?.ToString() ?? "#")})";
        }
    }
}
