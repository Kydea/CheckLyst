using CheckListManager.Common.Logic.Models.CheckLists;
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

        public override void Check(ICheckList Prototype)
        {
            if (!Prototype.Expressions.ContainsKey(ExpressionCode)) throw new Exception("Expression not found: " + ExpressionCode);
            DataType = Prototype.Expressions[ExpressionCode].DataType;
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch)
        {
            return ElementsBranch.CheckList.Expressions[ExpressionCode].Value;
        }

        public override string ToString()
        {
            return $"{ExpressionCode}";
        }
    }
}
