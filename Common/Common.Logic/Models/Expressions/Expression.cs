using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Expression
    {
        public const int MAXIMUM_LENGTH = 100000;

        public string SerializedValue { get; set; }

        public string Error { get; set; }

        public bool Unique { get; set; }

        public Expression()
        {
            Symbols = new List<Symbol>();
        }

        public void Parse(ICheckList checkList)
        {
            if (SerializedValue?.Length >= MAXIMUM_LENGTH)
                throw new Parsing.ParseException($"Expressions lengh must not exceed {MAXIMUM_LENGTH - 1} characters", MAXIMUM_LENGTH);

            Root = new Parsing.TokenSet(SerializedValue, 0).Symbol;
            Root.Expression = this;
        }

        [JsonIgnore]
        public Symbol Root { get; private set; }

        [JsonIgnore]
        public List<Symbol> Symbols { get; }

        [JsonIgnore]
        public TypedValue Value { get; private set; }

        public string TypeName => Value?.Type.TypeName;

        public object RawValue => Value?.Value;

        public void Evaluate(ICheckList instance)
        {
            Value = Root.GetValue(instance);
        }

        public DataType DataType => Root?.DataType ?? DataType.Error;

	    public void Check(ICheckList prototype)
        {
            Error = null;
            Root.Check(prototype);
        }
	}
}
