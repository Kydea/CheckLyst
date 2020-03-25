using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.IteratorFunctions;
using CheckListManager.Common.Logic.Models.Expressions.ElementProperties;
using CheckListManager.Common.Logic.Models.Expressions.LogicOperators;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public abstract class Symbol
    {
        private Expression _Expression;
        [JsonIgnore]
        public virtual Expression Expression
        {
            get
            {
                return _Expression;
            }
            set
            {
                _Expression = value;
                _Expression.Symbols.Add(this);
            }
        }

        public abstract TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone);

        public abstract void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack);

        public override abstract string ToString();

        public DataType DataType { get; protected set; }

        public DataType? InnerDataType { get; protected set; }

        public int Position { get; set; }
    }
}
