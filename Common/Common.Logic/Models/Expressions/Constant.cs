using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Constant : Symbol
    {
        private readonly TypedValue _value;

        public Constant(TypedValue value)
        {
            _value = value;
        }

        public override void Check(Prototype prototype)
        {
            DataType = _value.Type;
        }

        public override TypedValue GetValue(IElementsBranch elementsBranch)
        {
            return _value;
        }
    }
}
