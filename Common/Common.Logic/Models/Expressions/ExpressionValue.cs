using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class ExpressionValue : Symbol
    {
        private readonly Expression _otherExpression;

        public ExpressionValue(Expression otherExpression)
        {
            _otherExpression = otherExpression;
        }

        public override void Check(ICheckList prototype)
        {
            DataType = _otherExpression.DataType;
        }

        public override TypedValue GetValue(IElementsBranch elementsBranch)
        {
            return _otherExpression.Value;
        }
    }
}
