using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions;
using CheckListManager.Common.Logic.Models.Expressions.ElementProperties;
using CheckListManager.Common.Logic.Models.Expressions.LogicOperators;
using Newtonsoft.Json;
using System.Linq;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class Symbol
    {
        private Expression _Expression;
        [JsonIgnore]
        public virtual Expression Expression
        {
            get => _Expression;
            set
            {
                _Expression = value;
                _Expression.Symbols.Add(this);
            }
        }

        public static List<TypedValue> GetIteratorValues(ICheckListGroup Iterator, Symbol Argument, Dictionary<string, int> IterationIndexes, string TimeZone)
        {
            IterationIndexes[Iterator.Code] = 0;
            return Iterator.Iterations.Where(x => !x.Removed).Select(Iteration => {
                var Value = Argument.GetValue(Iteration, IterationIndexes, TimeZone);
                IterationIndexes[Iterator.Code]++;
                return Value;
            }).ToList();
        }

        public abstract TypedValue GetValue(IElementsBranch ElementsBranch, Dictionary<string, int> IterationIndexes, string TimeZone);
        public abstract void BuildDependencies(ICheckList CheckList, Expression Expression);
        public abstract void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack);
        public override abstract string ToString();
        public DataType DataType { get; protected set; }
        public DataType? InnerDataType { get; protected set; }
        public int Position { get; set; }
    }
}
