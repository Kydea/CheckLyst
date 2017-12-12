using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;

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
            Root = new Parsing.TokenSet(SerializedValue, 0).Symbol;
            Root.Expression = this;
        }

        [JsonIgnore]
        public Symbol Root { get; private set; }

        [JsonIgnore]
        public List<Symbol> Symbols { get; }

        [JsonIgnore]
        public TypedValue Value { get; private set; }

        public void Evaluate(ICheckList instance)
        {
            Value = Root.GetValue(instance);
        }

        public DataType DataType => Root?.DataType ?? DataType.Error;

	    public void Check(ICheckList prototype)
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
			return SerializedValue?.GetHashCode() ?? 0;
		}
	}
}
