using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class UnknowDataTypeException : ParseException
    {
        public UnknowDataTypeException(Position Position) : base(null, Position)
        {
        }
   }

}