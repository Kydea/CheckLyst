using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public abstract class UnaryIteratorFunction : UnaryOperator
    {
        public override void BuildDependencies(ICheckList CheckList, Expression Expression)
        {
            if (!CheckList.Elements.TryGetValue(IteratorCode, out var Iterator)) throw new ParseException($"Element with code {IteratorCode} not found", Position);
            var DependingElement = (DependencyNode)Iterator;
            Expression.AddDependingRelation(DependencyRelation.Properties.Value, DependingElement, DependencyRelation.Properties.Iterations);
            base.BuildDependencies(CheckList, Expression);
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            var Element = ElementProperties.ElementProperty.Check(CheckList, IteratorStack, IteratorCode, Position);
            if (!(Element is ICheckListGroup Group)) throw new ParseException("The element with code " + IteratorCode + " is not a Group", Position);
            if (!Group.Iterator) throw new ParseException("The group " + IteratorCode + " is not an Iterator", Position);

            if (Group.IterationItem)
            {
                IteratorStack.Add(IteratorCode);
                base.Check(CheckList, Expression, IteratorStack);
                IteratorStack.RemoveAt(IteratorStack.Count - 1);
            }
            else
                base.Check(CheckList, Expression, new List<string> { IteratorCode });
        }

        public string IteratorCode { get; set; }

        protected abstract string Property { get; }

        protected abstract TypedValue GetValue(ICheckListGroup Iterator, List<TypedValue> Values);
        public override TypedValue GetValue(IElementsBranch ElementsBranch, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            var Iterator = (ICheckListGroup)ElementsBranch.Elements[IteratorCode];
            var Values = GetIteratorValues(Iterator, Argument, IterationIndexes, TimeZone);
            return Values.FirstOrDefault(TypedValue => TypedValue.Type == DataType.Error) ?? GetValue(Iterator, Values);
        }

        public override string ToString()
        {
            return $"{IteratorCode}.{Property}({Argument?.ToString() ?? "#"})";
        }
    }
}
