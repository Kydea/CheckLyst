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
        public enum Uniqueness { no, uniqueOverInProgressInstances, uniqueOverAllInstances }

        public string Code { get; set; }
        public string SerializedValue { get; set; }
        public string Description { get; set; }
        private string _Error;
        public string Error
        {
            get => _Error;
            set
            {
                _Error = value;
                ErrorPosition = null;
                if (value != null)
                    Root = null;
            }
        }
        public Position ErrorPosition { get; set; }
        public Uniqueness Unique { get; set; }
        public bool Exception { get; set; }
        public bool Notification { get; set; }
        public string NotificationEmail { get; set; }
        public string NotificationNote { get; set; }
        public bool Notified { get; set; }

        public Expression()
        {
            Symbols = new List<Symbol>();
        }

        public void Validate(ICheckList checkList, string Code)
        {
            Error = null;
            this.Code = Code;

            if (string.IsNullOrWhiteSpace(Description) && (Unique != Uniqueness.no || Exception || Notification))
                throw new Exception($"Unique, exception and notification expressions must have a description");
            if (SerializedValue?.Length >= MAXIMUM_LENGTH)
                throw new Exception($"Expressions length must not exceed {MAXIMUM_LENGTH - 1} characters");
            if (Code.Length > 50)
                throw new Exception($"Expression codes must not exceed 50 characters");
            if (Description?.Length > 100)
                throw new Exception($"Expression descriptions must not exceed 100 characters");
        }

        public void Parse(ICheckList checkList)
        {
            Root = new TokenSet(SerializedValue, new Position()).Symbol;
            Root.Expression = this;
            Value = Value ?? new TypedValue(DataType.Error, null);
        }

        [JsonIgnore]
        public Symbol Root { get; private set; }

        [JsonIgnore]
        public List<Symbol> Symbols { get; }

        public TypedValue Value { get; set; }

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
                Error = Exception.Message;
                ErrorPosition = Exception.Position;
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
                if (Error != null) return false;

                Root.Check(CheckList, this, new List<string>());
                if ((Exception || Notification) && Root.DataType != DataType.Boolean && Root.DataType != DataType.Date && Root.DataType != DataType.DateTime)
                    throw new Exception($"Exception and notification expressions must be boolean or date or date-time");

                if (Unique != Uniqueness.no && Root.DataType == DataType.Array)
                    throw new Exception($"Unique expressions cannot be arrays");

                return true;
            }
            catch (UnknowDataTypeException Exception)
            {
                ErrorPosition = Exception.Position;
                return false;
            }
            catch (ParseException Exception)
            {
                Error = Exception.Message;
                ErrorPosition = Exception.Position;
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
            TypedValue NewValue;
            try
            {
                NewValue = Root.GetValue(instance, new Dictionary<string, int>(), TimeZone);
            }
            catch(Exception Exception)
            {
                NewValue = new TypedValue(DataType.Error, Exception.Message);
            }

            if (
                Notification
                && !(Value.IsNull && NewValue.IsNull)
                && (
                    (Value.IsNull ^ NewValue.IsNull)
                    || Value.Type == DataType.Boolean && Value.BooleanValue != NewValue.BooleanValue
                    || Value.Type == DataType.Date && Value.DateTimeValue != NewValue.DateTimeValue
                    || Value.Type == DataType.DateTime && Value.DateTimeValue != NewValue.DateTimeValue
                )
            )
            {
                instance.NeedToSaveExpressionValues = true;
                Notified = false; //when the value changes the notification is reset
            }
            Value = NewValue;
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
