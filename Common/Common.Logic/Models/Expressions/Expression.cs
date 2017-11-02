using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using LinqExpression = System.Linq.Expressions.Expression;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuspiciousTypeConversion.Global

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Expression
    {
        public string SerializedValue { get; set; }

        public string Error { get; set; }

        public bool Unique { get; set; }

        public Expression()
        {
            Symbols = new List<Symbol>();
        }

        public void Parse(ICheckList checkList)
        {
            Parse(this, SerializedValue, checkList);
            Root.Expression = this;
        }

        [JsonIgnore]
        public Symbol Root { get; private set; }

        [JsonIgnore]
        public List<Symbol> Symbols { get; }

        [JsonIgnore]
        public TypedValue Value { get; private set; }

        public void Evaluate(Instance instance)
        {
            Value = Root.GetValue(instance);
        }

        public DataType DataType => Root?.DataType ?? DataType.Error;

	    public void Check(Prototype prototype)
        {
            try
            {
                Error = null;
                Root.Check(prototype);
            }
            catch (Exception exception)
            {
                Error = exception.Message;
            }
        }

		public override bool Equals(object obj)
		{
			return obj != null && ((Expression)obj).SerializedValue == SerializedValue;
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return SerializedValue?.GetHashCode() ?? 0;
		}

		#region Static

		private static void Parse(Expression expression, string expressionString, ICheckList checkList)
        {
            expression.Root = Symbol.Parse(ToLambda(expressionString, checkList), checkList);
        }

        public static LambdaExpression ToLambda(string expression, ICheckList checkList)
        {
            if (expression == null) throw new Exception("Fill the expression");

            var parameters = new List<ParameterExpression>(checkList.Elements.Count + checkList.Expressions.Count);
            foreach (var element in checkList.Elements)
                parameters.Add(LinqExpression.Parameter((element.Value as Group)?.Elements.Values.GetType() ?? element.Value.GetType(), element.Key));
			foreach (var e in checkList.Expressions)
				parameters.Add(LinqExpression.Parameter(e.Value.GetType(), e.Key));
			return DynamicExpression.ParseLambda(parameters.ToArray(), null, expression);
        }

		#endregion

		#region Expression

		public static bool operator ==(Expression a, Expression b) { return Equals(a, null) && Equals(b, null) || (Equals(a, null) ? b.Equals(null) : a.Equals(b)); }

        public static bool operator !=(Expression a, Expression b) { return !(a == b); }
		public static bool operator <=(Expression a, Expression b) { return a <= b; }
		public static bool operator >=(Expression a, Expression b) { return a >= b; }
		public static bool operator <(Expression a, Expression b) { return a < b; }
		public static bool operator >(Expression a, Expression b) { return a > b; }
		public static Expression operator !(Expression value) { return value; }

		public static Expression operator *(Expression a, Expression b) { return null; }
		public static Expression operator /(Expression a, Expression b) { return null; }

		public static Expression operator +(Expression a, Expression b) { return null; }
		public static Expression operator -(Expression a, Expression b) { return null; }
		public static Expression operator &(Expression a, Expression b) { return null; }
		public static Expression operator |(Expression a, Expression b) { return null; }

		public static bool operator true(Expression a) { return true; }
		public static bool operator false(Expression a) { return false; }

		#endregion

		#region String

		public static bool operator ==(Expression a, string b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && a.Equals(b); }
		public static bool operator !=(Expression a, string b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && !a.Equals(b); }
		public static bool operator <=(Expression a, string b) { return a <= b; }
		public static bool operator >=(Expression a, string b) { return a >= b; }
		public static bool operator <(Expression a, string b) { return a < b; }
		public static bool operator >(Expression a, string b) { return a > b; }

        #endregion

        #region Boolean

        public static bool operator ==(Expression a, bool b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(Expression a, bool b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(Expression a, bool b) { return a <= b; }
        public static bool operator >=(Expression a, bool b) { return a >= b; }
        public static bool operator <(Expression a, bool b) { return a < b; }
        public static bool operator >(Expression a, bool b) { return a > b; }

		public static bool operator ==(bool a, Expression b) { return a.Equals(b); }
        public static bool operator !=(bool a, Expression b) { return !a.Equals(b); }
        public static bool operator <=(bool a, Expression b) { return a <= b; }
        public static bool operator >=(bool a, Expression b) { return a >= b; }
        public static bool operator <(bool a, Expression b) { return a < b; }
        public static bool operator >(bool a, Expression b) { return a > b; }

		public static implicit operator bool(Expression value) { return false; }

		public static implicit operator Expression(bool value) { return null; }

        #endregion

        #region Decimal

        public static bool operator ==(Expression a, decimal b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(Expression a, decimal b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(Expression a, decimal b) { return a <= b; }
        public static bool operator >=(Expression a, decimal b) { return a >= b; }
        public static bool operator <(Expression a, decimal b) { return a < b; }
        public static bool operator >(Expression a, decimal b) { return a > b; }

        public static bool operator ==(decimal a, Expression b) { return a.Equals(b); }
        public static bool operator !=(decimal a, Expression b) { return !a.Equals(b); }
        public static bool operator <=(decimal a, Expression b) { return a <= b; }
        public static bool operator >=(decimal a, Expression b) { return a >= b; }
        public static bool operator <(decimal a, Expression b) { return a < b; }
        public static bool operator >(decimal a, Expression b) { return a > b; }

        public static Expression operator *(Expression a, decimal b) { return null; }
		public static Expression operator /(Expression a, decimal b) { return null; }
		public static Expression operator +(Expression a, decimal b) { return null; }
		public static Expression operator -(Expression a, decimal b) { return null; }

        public static Expression operator *(decimal a, Expression b) { return null; }
        public static Expression operator /(decimal a, Expression b) { return null; }
        public static Expression operator +(decimal a, Expression b) { return null; }
        public static Expression operator -(decimal a, Expression b) { return null; }

        #endregion

        #region Int

        public static bool operator ==(Expression a, int b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(Expression a, int b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(Expression a, int b) { return a <= b; }
        public static bool operator >=(Expression a, int b) { return a >= b; }
        public static bool operator <(Expression a, int b) { return a < b; }
        public static bool operator >(Expression a, int b) { return a > b; }

        public static bool operator ==(int a, Expression b) { return a.Equals(b); }
        public static bool operator !=(int a, Expression b) { return !a.Equals(b); }
        public static bool operator <=(int a, Expression b) { return a <= b; }
        public static bool operator >=(int a, Expression b) { return a >= b; }
        public static bool operator <(int a, Expression b) { return a < b; }
        public static bool operator >(int a, Expression b) { return a > b; }

        public static implicit operator int(Expression value) { return 0; }

        public static implicit operator Expression(int value) { return null; }

        public static Expression operator *(Expression a, int b) { return null; }
        public static Expression operator /(Expression a, int b) { return null; }
        public static Expression operator +(Expression a, int b) { return null; }
        public static Expression operator -(Expression a, int b) { return null; }

        public static Expression operator *(int a, Expression b) { return null; }
        public static Expression operator /(int a, Expression b) { return null; }
        public static Expression operator +(int a, Expression b) { return null; }
        public static Expression operator -(int a, Expression b) { return null; }

        #endregion

        #region Float

        public static bool operator ==(Expression a, float b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(Expression a, float b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(Expression a, float b) { return a <= b; }
        public static bool operator >=(Expression a, float b) { return a >= b; }
        public static bool operator <(Expression a, float b) { return a < b; }
        public static bool operator >(Expression a, float b) { return a > b; }

        public static bool operator ==(float a, Expression b) { return a.Equals(b); }
        public static bool operator !=(float a, Expression b) { return !a.Equals(b); }
        public static bool operator <=(float a, Expression b) { return a <= b; }
        public static bool operator >=(float a, Expression b) { return a >= b; }
        public static bool operator <(float a, Expression b) { return a < b; }
        public static bool operator >(float a, Expression b) { return a > b; }

        public static Expression operator *(Expression a, float b) { return null; }
        public static Expression operator /(Expression a, float b) { return null; }
        public static Expression operator +(Expression a, float b) { return null; }
        public static Expression operator -(Expression a, float b) { return null; }

        public static Expression operator *(float a, Expression b) { return null; }
        public static Expression operator /(float a, Expression b) { return null; }
        public static Expression operator +(float a, Expression b) { return null; }
        public static Expression operator -(float a, Expression b) { return null; }

        #endregion

        #region Double

        public static bool operator ==(Expression a, double b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(Expression a, double b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(Expression a, double b) { return a <= b; }
        public static bool operator >=(Expression a, double b) { return a >= b; }
        public static bool operator <(Expression a, double b) { return a < b; }
        public static bool operator >(Expression a, double b) { return a > b; }

        public static bool operator ==(double a, Expression b) { return a.Equals(b); }
        public static bool operator !=(double a, Expression b) { return !a.Equals(b); }
        public static bool operator <=(double a, Expression b) { return a <= b; }
        public static bool operator >=(double a, Expression b) { return a >= b; }
        public static bool operator <(double a, Expression b) { return a < b; }
        public static bool operator >(double a, Expression b) { return a > b; }

        public static Expression operator *(Expression a, double b) { return null; }
        public static Expression operator /(Expression a, double b) { return null; }
        public static Expression operator +(Expression a, double b) { return null; }
        public static Expression operator -(Expression a, double b) { return null; }

        public static Expression operator *(double a, Expression b) { return null; }
        public static Expression operator /(double a, Expression b) { return null; }
        public static Expression operator +(double a, Expression b) { return null; }
        public static Expression operator -(double a, Expression b) { return null; }

        #endregion

        #region RawValue

        public static bool operator ==(Expression a, RawValue b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(Expression a, RawValue b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(Expression a, RawValue b) { return a <= b; }
        public static bool operator >=(Expression a, RawValue b) { return a >= b; }
        public static bool operator <(Expression a, RawValue b) { return a < b; }
        public static bool operator >(Expression a, RawValue b) { return a > b; }

        public static Expression operator *(Expression a, RawValue b) { return null; }
        public static Expression operator /(Expression a, RawValue b) { return null; }
        public static Expression operator +(Expression a, RawValue b) { return null; }
        public static Expression operator -(Expression a, RawValue b) { return null; }

        public static Expression operator &(Expression a, RawValue b) { return null; }
		public static Expression operator |(Expression a, RawValue b) { return null; }

		#endregion
	}
}
