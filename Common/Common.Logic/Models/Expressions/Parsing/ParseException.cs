using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class ParseException : Exception
    {
        public ParseException(string Message, Position Position) : base(Message)
        {
            this.Position = Position;
        }

        public Position Position { get; }
    }

}