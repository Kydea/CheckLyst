using System;
using CheckListManager.Common.Logic.Models.Expressions;

// ReSharper disable SuspiciousTypeConversion.Global

namespace CheckListManager.Common.Logic.Models
{
	public class RawValue
	{
		public string Value { get; }

		public RawValue() { }

		public RawValue(string value)
		{
			Value = value;
		}

		public override int GetHashCode()
		{
			return Value?.GetHashCode() ?? 0;
		}

		public override bool Equals(object obj)
		{
			return obj is RawValue && obj.ToString() == ToString();
		}

		public override string ToString()
		{
			return Value;
		}

		#region RawValue

		public static bool operator ==(RawValue a, RawValue b)
		{
			return Equals(a, null) && Equals(b, null) || !Equals(a, null) && a.Equals(b);
		}

		public static bool operator !=(RawValue a, RawValue b)
		{
			return Equals(a, null) && Equals(b, null) || !Equals(a, null) && !a.Equals(b);
		}

		public static bool operator <=(RawValue a, RawValue b)
		{
			return a <= b;
		}

		public static bool operator >=(RawValue a, RawValue b)
		{
			return a >= b;
		}

		public static bool operator <(RawValue a, RawValue b)
		{
			return a < b;
		}

		public static bool operator >(RawValue a, RawValue b)
		{
			return a > b;
		}

		public static RawValue operator !(RawValue value)
		{
			return value;
		}

		public static RawValue operator *(RawValue a, RawValue b)
		{
			return string.Empty;
		}

		public static RawValue operator /(RawValue a, RawValue b)
		{
			return string.Empty;
		}

		public static RawValue operator +(RawValue a, RawValue b)
		{
			return string.Empty;
		}

		public static RawValue operator -(RawValue a, RawValue b)
		{
			return string.Empty;
		}

		public static RawValue operator &(RawValue a, RawValue b)
		{
			return string.Empty;
		}

		public static RawValue operator |(RawValue a, RawValue b)
		{
			return string.Empty;
		}

		public static bool operator true(RawValue a)
		{
			return true;
		}

		public static bool operator false(RawValue a)
		{
			return false;
		}

		#endregion

		#region String

		public static bool operator ==(RawValue a, string b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && a.Equals(b); }
		public static bool operator !=(RawValue a, string b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && !a.Equals(b); }
		public static bool operator <=(RawValue a, string b) { return a <= b; }
		public static bool operator >=(RawValue a, string b) { return a >= b; }
		public static bool operator <(RawValue a, string b) { return a < b; }
		public static bool operator >(RawValue a, string b) { return a > b; }

        public static bool operator ==(string a, RawValue b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(string a, RawValue b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(string a, RawValue b) { return a <= b; }
        public static bool operator >=(string a, RawValue b) { return a >= b; }
        public static bool operator <(string a, RawValue b) { return a < b; }
        public static bool operator >(string a, RawValue b) { return a > b; }


        public static implicit operator RawValue(string value) { return new RawValue(value); }

		public static implicit operator string(RawValue value) { return value?.Value; }

        #endregion

        #region Boolean

        public static bool operator ==(RawValue a, bool b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(RawValue a, bool b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(RawValue a, bool b) { return a <= b; }
        public static bool operator >=(RawValue a, bool b) { return a >= b; }
        public static bool operator <(RawValue a, bool b) { return a < b; }
        public static bool operator >(RawValue a, bool b) { return a > b; }

		public static bool operator ==(bool a, RawValue b) { return a.Equals(b); }
        public static bool operator !=(bool a, RawValue b) { return !a.Equals(b); }
        public static bool operator <=(bool a, RawValue b) { return a <= b; }
        public static bool operator >=(bool a, RawValue b) { return a >= b; }
        public static bool operator <(bool a, RawValue b) { return a < b; }
        public static bool operator >(bool a, RawValue b) { return a > b; }


		public static implicit operator bool(RawValue value) { return false; }

		public static implicit operator RawValue(bool value) { return string.Empty; }

        #endregion

        #region Decimal

        public static bool operator ==(RawValue a, decimal b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(RawValue a, decimal b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(RawValue a, decimal b) { return a <= b; }
        public static bool operator >=(RawValue a, decimal b) { return a >= b; }
        public static bool operator <(RawValue a, decimal b) { return a < b; }
        public static bool operator >(RawValue a, decimal b) { return a > b; }

        public static bool operator ==(decimal a, RawValue b) { return a.Equals(b); }
        public static bool operator !=(decimal a, RawValue b) { return !a.Equals(b); }
        public static bool operator <=(decimal a, RawValue b) { return a <= b; }
        public static bool operator >=(decimal a, RawValue b) { return a >= b; }
        public static bool operator <(decimal a, RawValue b) { return a < b; }
        public static bool operator >(decimal a, RawValue b) { return a > b; }

        public static RawValue operator *(RawValue a, decimal b) { return string.Empty; }
        public static RawValue operator /(RawValue a, decimal b) { return string.Empty; }
        public static RawValue operator +(RawValue a, decimal b) { return string.Empty; }
        public static RawValue operator -(RawValue a, decimal b) { return string.Empty; }

        public static RawValue operator *(decimal a, RawValue b) { return string.Empty; }
        public static RawValue operator /(decimal a, RawValue b) { return string.Empty; }
        public static RawValue operator +(decimal a, RawValue b) { return string.Empty; }
        public static RawValue operator -(decimal a, RawValue b) { return string.Empty; }

        #endregion

        #region Int

        public static bool operator ==(RawValue a, int b) { return !Equals(a, null) && a.Equals(b); }
		public static bool operator !=(RawValue a, int b) { return !Equals(a, null) && !a.Equals(b); }
		public static bool operator <=(RawValue a, int b) { return a <= b; }
		public static bool operator >=(RawValue a, int b) { return a >= b; }
		public static bool operator <(RawValue a, int b) { return a < b; }
		public static bool operator >(RawValue a, int b) { return a > b; }

        public static bool operator ==(int a, RawValue b) { return a.Equals(b); }
        public static bool operator !=(int a, RawValue b) { return !a.Equals(b); }
        public static bool operator <=(int a, RawValue b) { return a <= b; }
        public static bool operator >=(int a, RawValue b) { return a >= b; }
        public static bool operator <(int a, RawValue b) { return a < b; }
        public static bool operator >(int a, RawValue b) { return a > b; }

        public static implicit operator int(RawValue value) { try { return Convert.ToInt32(value.Value); } catch { return 0; } }

		public static implicit operator RawValue(int value) { return new RawValue(value.ToString()); }

        public static RawValue operator *(RawValue a, int b) { return string.Empty; }
		public static RawValue operator /(RawValue a, int b) { return string.Empty; }
		public static RawValue operator +(RawValue a, int b) { return string.Empty; }
		public static RawValue operator -(RawValue a, int b) { return string.Empty; }

        public static RawValue operator *(int a, RawValue b) { return string.Empty; }
        public static RawValue operator /(int a, RawValue b) { return string.Empty; }
        public static RawValue operator +(int a, RawValue b) { return string.Empty; }
        public static RawValue operator -(int a, RawValue b) { return string.Empty; }

        #endregion

        #region Float

        public static bool operator ==(RawValue a, float b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(RawValue a, float b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(RawValue a, float b) { return a <= b; }
        public static bool operator >=(RawValue a, float b) { return a >= b; }
        public static bool operator <(RawValue a, float b) { return a < b; }
        public static bool operator >(RawValue a, float b) { return a > b; }

        public static bool operator ==(float a, RawValue b) { return a.Equals(b); }
        public static bool operator !=(float a, RawValue b) { return !a.Equals(b); }
        public static bool operator <=(float a, RawValue b) { return a <= b; }
        public static bool operator >=(float a, RawValue b) { return a >= b; }
        public static bool operator <(float a, RawValue b) { return a < b; }
        public static bool operator >(float a, RawValue b) { return a > b; }

        public static RawValue operator *(RawValue a, float b) { return string.Empty; }
        public static RawValue operator /(RawValue a, float b) { return string.Empty; }
        public static RawValue operator +(RawValue a, float b) { return string.Empty; }
        public static RawValue operator -(RawValue a, float b) { return string.Empty; }

        public static RawValue operator *(float a, RawValue b) { return string.Empty; }
        public static RawValue operator /(float a, RawValue b) { return string.Empty; }
        public static RawValue operator +(float a, RawValue b) { return string.Empty; }
        public static RawValue operator -(float a, RawValue b) { return string.Empty; }

        #endregion

        #region Double

        public static bool operator ==(RawValue a, double b) { return !Equals(a, null) && a.Equals(b); }
        public static bool operator !=(RawValue a, double b) { return !Equals(a, null) && !a.Equals(b); }
        public static bool operator <=(RawValue a, double b) { return a <= b; }
        public static bool operator >=(RawValue a, double b) { return a >= b; }
        public static bool operator <(RawValue a, double b) { return a < b; }
        public static bool operator >(RawValue a, double b) { return a > b; }

        public static bool operator ==(double a, RawValue b) { return a.Equals(b); }
        public static bool operator !=(double a, RawValue b) { return !a.Equals(b); }
        public static bool operator <=(double a, RawValue b) { return a <= b; }
        public static bool operator >=(double a, RawValue b) { return a >= b; }
        public static bool operator <(double a, RawValue b) { return a < b; }
        public static bool operator >(double a, RawValue b) { return a > b; }

        public static RawValue operator *(RawValue a, double b) { return string.Empty; }
        public static RawValue operator /(RawValue a, double b) { return string.Empty; }
        public static RawValue operator +(RawValue a, double b) { return string.Empty; }
        public static RawValue operator -(RawValue a, double b) { return string.Empty; }

        public static RawValue operator *(double a, RawValue b) { return string.Empty; }
        public static RawValue operator /(double a, RawValue b) { return string.Empty; }
        public static RawValue operator +(double a, RawValue b) { return string.Empty; }
        public static RawValue operator -(double a, RawValue b) { return string.Empty; }

        #endregion

        #region Expression

        public static bool operator ==(RawValue a, Expression b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && a.Equals(b); }
		public static bool operator !=(RawValue a, Expression b) { return Equals(a, null) && Equals(b, null) || !Equals(a, null) && !a.Equals(b); }
		public static bool operator <=(RawValue a, Expression b) { return false; }
		public static bool operator >=(RawValue a, Expression b) { return false; }
		public static bool operator <(RawValue a, Expression b) { return false; }
		public static bool operator >(RawValue a, Expression b) { return false; }

        public static RawValue operator *(RawValue a, Expression b) { return string.Empty; }
		public static RawValue operator /(RawValue a, Expression b) { return string.Empty; }
		public static RawValue operator +(RawValue a, Expression b) { return string.Empty; }
		public static RawValue operator -(RawValue a, Expression b) { return string.Empty; }

        public static RawValue operator &(RawValue a, Expression b) { return string.Empty; }
		public static RawValue operator |(RawValue a, Expression b) { return string.Empty; }

        #endregion
    }
}