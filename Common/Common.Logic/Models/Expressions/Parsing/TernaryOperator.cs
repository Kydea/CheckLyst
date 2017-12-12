using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class TernaryOperator : Token
    {
        public TernaryOperator(ref int Position, int Index) : base(Position, Index)
        {
            Symbol = new Expressions.TernaryOperator();
        }

        public override string ToString()
        {
            return $"?:";
        }

        public override int TokenOrder => 8;

        public override bool TokenOrderRightToLeft => true;
    }
}