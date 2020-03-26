using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.Expressions
{
    public class Expression : IDependencyNode
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
            DependingNodes.Clear();
            DependentNodes.Clear();
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

        public void Evaluate(ICheckList instance, string timeZone)
        {
            Value = Root.GetValue(instance, timeZone);
        }

        public bool BuildDependencies(ICheckList checklist)
        {
            try
            {
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

        private string DependencyChainKey { get; set; }
        public void TraverseDependencyChain(ICheckList CheckList, List<IDependencyNode> DependencyChain, string DependencyChainKey)
        {
            DependencyChain.Add(this);
            if (this.DependencyChainKey == DependencyChainKey)
                throw new Exception($"Dependency cycle found: {string.Join(" > ", DependencyChain.Select(e => e.DependencyNodeKey.Description))}");
            this.DependencyChainKey = DependencyChainKey;

            foreach (var DependingNodeKey in DependingNodes)
                DependingNodeKey.GetNode(CheckList).TraverseDependencyChain(CheckList, DependencyChain, DependencyChainKey);

            Check(CheckList);
        }

        private bool Checked { get; set; }
        public bool Check(ICheckList CheckList)
        {
            try
            {
                if (Checked) return true;
                Checked = true;
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

        public DataType DataType => Root?.DataType ?? DataType.Error;

        public DataType? InnerDataType => Root?.InnerDataType;

        public DependencyNodeKey DependencyNodeKey => new DependencyNodeKey("expression", Code);
        public List<DependencyNodeKey> DependentNodes { get; } = new List<DependencyNodeKey>();
        public List<DependencyNodeKey> DependingNodes { get; } = new List<DependencyNodeKey>();
    }
}
