using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Constant : Symbol
    {
        private TypedValue Value;

        public Constant(TypedValue Value)
        {
            this.Value = Value;
        }

        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            DataType = Value.Type;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch)
        {
            return Value;
        }

        public override string ToString()
        {
            return $"{(Value.IsNull ? "(null)" : $"'{Value.Value.ToString()}'")}";
        }
    }
}
