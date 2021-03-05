using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Common.Logic.Models.Expressions.Parsing
{
    public class Position
    {
        public int Index { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }

        public Position() : this(0, 0, 0) { }

        public Position(Position Position) : this(Position.Index, Position.Row, Position.Column) { }

        private Position(int Index, int Row, int Column)
        {
            this.Index = Index;
            this.Row = Row;
            this.Column = Column;
        }


        public void Increment(bool Row)
        {
            ++Index;
            if (Row)
            {
                Column = 0;
                ++this.Row;
            }
            else
                ++Column;
        }

        public override string ToString() => $"({Index}, {Row}, {Column})";
    }

}