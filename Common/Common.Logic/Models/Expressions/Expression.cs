using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Expression
    {
        public const int MAXIMUM_LENGTH = 10000;

        public string Code { get; set; }
        public string SerializedValue { get; set; }
        public string Description { get; set; }
        public string Error { get; set; }
        public bool Unique { get; set; }
        public bool Exception { get; set; }

        public Expression()
        {
            Symbols = new List<Symbol>();
        }

        public void Validate(ICheckList checkList)
        {
            if (string.IsNullOrWhiteSpace(Description) && (Unique || Exception))
                throw new Exception($"Unique expressions and exception expressions must have a description");

            if (SerializedValue?.Length >= MAXIMUM_LENGTH)
                throw new Parsing.ParseException($"Expressions lengh must not exceed {MAXIMUM_LENGTH - 1} characters", MAXIMUM_LENGTH);
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

        public string TypeName => Value?.Type.TypeName;

        public object RawValue => Value?.Value;

        public void Evaluate(ICheckList instance, string TimeZone)
        {
            Value = Root.GetValue(instance, TimeZone);
        }

        public DataType DataType => Root?.DataType ?? DataType.Error;

        public DataType? InnerDataType => Root?.InnerDataType;

        public void Check(ICheckList prototype)
        {
            Error = null;
            Root.Check(prototype, this, new List<string>());
            if (Exception && Root.DataType != DataType.Boolean)
                throw new Exception($"Exception expressions must be boolean");
        }

        public DependencyNodeKey DependencyNodeKey => DependencyNodeKey
        public List<DependencyNodeKey> DependentNodes { get; } = new List<DependencyNodeKey>();
        public List<DependencyNodeKey> DependingNodes { get; } = new List<DependencyNodeKey>();
    }
}
