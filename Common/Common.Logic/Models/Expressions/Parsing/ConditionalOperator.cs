using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class ConditionalOperator : Token
    {
        public ConditionalOperator(Position Position, int Index) : base(Position, Index)
        {
            Symbol = new Expressions.ConditionalOperator { Position = Position };
        }

        public override string ToString()
        {
            return $"?:";
        }

        public override int TokenOrder => 8;

        public override bool TokenOrderRightToLeft => true;
    }
}