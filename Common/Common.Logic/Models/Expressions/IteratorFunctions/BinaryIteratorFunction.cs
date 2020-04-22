using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions
{
    public abstract class BinaryIteratorFunction : BinaryOperator
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

            IteratorStack.Add(IteratorCode);
            base.Check(CheckList, Expression, IteratorStack);
            IteratorStack.RemoveAt(IteratorStack.Count - 1);
        }

        public string IteratorCode { get; set; }

        protected abstract TypedValue GetValue(List<TypedValue> LeftValues, List<TypedValue> RigheValues);

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var Iterator = (ICheckListGroup)ElementsBranch.Elements[IteratorCode];
            var LeftValues = Iterator.Iterations.Where(x => !x.Removed).Select(Iteration => Left.GetValue(Iteration, TimeZone)).ToList();
            var RightValues = Iterator.Iterations.Where(x => !x.Removed).Select(Iteration => Right.GetValue(Iteration, TimeZone)).ToList();
            return 
                LeftValues.FirstOrDefault(TypedValue => TypedValue.Type == DataType.Error) ??
                RightValues.FirstOrDefault(TypedValue => TypedValue.Type == DataType.Error) ??
                GetValue(LeftValues, RightValues);
        }
    }
}
