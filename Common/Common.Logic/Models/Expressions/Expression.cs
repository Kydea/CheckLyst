using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Expression : DependencyNode
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

        public void Validate(ICheckList checkList, string Code)
        {
            if (string.IsNullOrWhiteSpace(Description) && (Unique || Exception))
                throw new Exception($"Unique expressions and exception expressions must have a description");

            if (SerializedValue?.Length >= MAXIMUM_LENGTH)
                throw new ParseException($"Expressions lengh must not exceed {MAXIMUM_LENGTH - 1} characters", MAXIMUM_LENGTH);

            this.Code = Code;
        }

        public void Parse(ICheckList checkList)
        {
            Error = null;
            Root = new TokenSet(SerializedValue, 0).Symbol;
            Root.Expression = this;
            Value = Value ?? new TypedValue(DataType.Error, null);
        }

        [JsonIgnore]
        public Symbol Root { get; private set; }

        [JsonIgnore]
        public List<Symbol> Symbols { get; }

        public TypedValue Value { get; set; }

        public string TypeName => Value?.Type.TypeName;

        public object RawValue => Value?.Value;

        public Func<Expression, bool> DependencyChainAction { private get; set; }

        public bool BuildDependencies(ICheckList checklist)
        {
            try
            {
                if (Root == null) Parse(checklist);
                Root.BuildDependencies(checklist, this);
                return true;
            }
            catch (ParseException Exception)
            {
                Error = $"{Exception.Message} (position {Exception.Position})";
                return false;
            }
            catch (Exception Exception)
            {
                Error = Exception.Message;
                return false;
            }
        }

        public bool Check(ICheckList CheckList)
        {
            try
            {
                Root.Check(CheckList, this, new List<string>());
                if (Exception && Root.DataType != DataType.Boolean)
                    throw new Exception($"Exception expressions must be boolean");
                return true;
            }
            catch (ParseException Exception)
            {
                Error = $"{Exception.Message} (position {Exception.Position})";
                return false;
            }
            catch (Exception Exception)
            {
                Error = Exception.Message;
                return false;
            }
        }

        public void Evaluate(ICheckList instance, string TimeZone)
        {
            if (Root == null) Parse(instance);
            Value = Root.GetValue(instance, TimeZone);
        }

        public override bool Update(DependencyRelation.Properties Property)
        {
            if (Property != DependencyRelation.Properties.Value) throw new ArgumentException();
            return DependencyChainAction(this);
        }

        public DataType DataType => Root?.DataType ?? DataType.Error;

        public DataType? InnerDataType => Root?.InnerDataType;

        public override string DependencyClassName => "expression";
        public override string DependencyObjectCode => Code;

        public override string ToString() => $"expression {Code}";
    }
}
