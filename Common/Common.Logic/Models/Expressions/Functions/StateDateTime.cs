using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.Functions
{
    public class StateDateTime : UnaryOperator
    {
        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            base.Check(CheckList, Expression, IteratorStack);

            if (Argument.DataType != DataType.Text)
                throw new ParseException("The argument of StateDateTime must be a text", Position);

            DataType = DataType.DateTime;
        }

        public override void BuildDependencies(ICheckList CheckList, Expression Expression)
        {
            base.BuildDependencies(CheckList, Expression);
            Expression.AddDependingRelation(DependencyRelation.Properties.Value, (DependencyNode)CheckList, DependencyRelation.Properties.StateChange);
        }

        protected override TypedValue Evaluate(IElementsBranch ElementsBranch, TypedValue ArgumentValue)
        {
            var State = ArgumentValue.TextValue;
            var Instance = (IInstance)ElementsBranch.CheckList;
            return
                State == Data.InstanceState.Draft ? new TypedValue(DataType.DateTime, Instance.CreatedOn) :
                State == Data.InstanceState.Progress ? new TypedValue(DataType.DateTime, Instance.InProgressOn) :
                new TypedValue(DataType.Error, $"Invalid state {State}");
        }

        public override string ToString() =>
            $"{nameof(StateDateTime)}({Argument?.ToString() ?? "#"})";
    }
}
