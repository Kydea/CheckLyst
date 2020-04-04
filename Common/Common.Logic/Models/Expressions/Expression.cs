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
            Evaluated = false;
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

        private bool Evaluated { get; set; } = true;

        public void Evaluate(ICheckList instance, string timeZone)
        {
            if (Evaluated) return;
            if (Root == null) Parse(instance);

            Value = Root.GetValue(instance, timeZone);
            Evaluated = true; ;
        }

        public void ClearDependencies()
        {
            DependingNodes.Clear();
            DependentNodes.Clear();
        }

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

        private bool DependencyChainTouched = false;
        public bool TraverseDependencyChain(ICheckList CheckList, Stack<string> DependencyChain, DependencyNodeKey.Processes Process, Func<IDependencyNode, bool> DependencyChainAction)
        {
            if (Evaluated) return true;

            DependencyChain.Push(DependencyNodeKey.ToString());
            if (DependencyChainTouched)
                throw new Exception($"Dependency cycle found: {string.Join(" > ", DependencyChain.Reverse())}");
            DependencyChainTouched = true;

            var Result = true;
            foreach (var DependencyNodeKey in DependingNodes)
                if (!DependencyNodeKey.GetNode(CheckList).TraverseDependencyChain(CheckList, DependencyChain, DependencyNodeKey.Process, DependencyChainAction))
                    Result = false;

            if (!DependencyChainAction(this)) Result = false;
            DependencyChain.Pop();
            DependencyChainTouched = false;
            return Result;
        }

        public void MarkNodeAsChanged(ICheckList CheckList, List<IDependencyNode> Roots, DependencyNodeKey.Processes Process)
        {
            if (!Evaluated) return;
            Evaluated = false;
            if (DependentNodes.Any())
                foreach (var DependencyNodeKey in DependentNodes)
                    DependencyNodeKey.GetNode(CheckList).MarkNodeAsChanged(CheckList, Roots, DependencyNodeKey.Process);
            else
                Roots.Add(this);
        }

        private bool Checked = false;
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
        public HashSet<DependencyNodeKey> DependentNodes { get; } = new HashSet<DependencyNodeKey>();
        public HashSet<DependencyNodeKey> DependingNodes { get; } = new HashSet<DependencyNodeKey>();
    }
}
