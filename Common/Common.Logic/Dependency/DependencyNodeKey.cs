using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Dependency
{
    public class DependencyNodeKey
    {
        public enum Processes { Value, Validity, ReadOnly, Hidden, Other }

        public DependencyNodeKey()
        { }

        public DependencyNodeKey(string ClassName, string ObjectCode, string PropertyName = null)
        {
            this.ClassName = ClassName;
            this.ObjectCode = ObjectCode;
            this.PropertyName = PropertyName;
        }

        public DependencyNodeKey(string ClassName, int ObjectID, string PropertyName = null)
        {
            this.ClassName = ClassName;
            this.ObjectID = ObjectID;
            this.PropertyName = PropertyName;
        }

        public string ClassName { get; set; }
        public string ObjectCode { get; set; }
        public int? ObjectID { get; set; }
        public Processes Process { get; private set; } = Processes.Other;
        public string PropertyName { get; set; }

        public string Description => ToString();

        public string Key => $"{ClassName}-{ObjectCode ?? ObjectID?.ToString()}-{Process}-{PropertyName}";

        public DependencyNodeKey Clone(Processes Process, string PropertyName)
            => new DependencyNodeKey { ClassName = ClassName, ObjectCode = ObjectCode, ObjectID = ObjectID, Process = Process, PropertyName = PropertyName };

        public override bool Equals(object o)
            => Equals(o as DependencyNodeKey);

        public bool Equals(DependencyNodeKey o)
            => o != null && o.ClassName == ClassName && o.ObjectCode == ObjectCode && o.ObjectID == ObjectID && Process == o.Process && o.PropertyName == PropertyName;

        public override int GetHashCode()
            => Key.GetHashCode();

        public override string ToString()
            => $"{(PropertyName == null ? string.Empty : $"{PropertyName} of the ")}{ClassName} {ObjectCode ?? ObjectID?.ToString()}{ProcessDescription}";

        private string ProcessDescription
            => 
                ClassName == "expressionNO" ? string.Empty : 
                Process == Processes.Hidden ? " (Hidden)" :
                Process == Processes.ReadOnly ? " (ReadOnly)" :
                Process == Processes.Value ? " (Value)" :
                Process == Processes.Validity ? " (Validity)" :
                string.Empty;


        public IDependencyNode GetNode(ICheckList checkList)
        {
            switch (ClassName)
            {
                case "expression":
                    return checkList.Expressions[ObjectCode];
                case "element":
                    return checkList.Elements[ObjectCode];
                case "item":
                case "group":
                    return ObjectID.HasValue ? checkList.Items[ObjectID.Value] : checkList.Elements[ObjectCode];
                case "checklist":
                    return checkList;
                case "page":
                    return ObjectID.HasValue ? checkList.Pages[ObjectID.Value] : checkList.Elements[ObjectCode];
                default:
                    throw new Exception($"Invalid class name {ClassName}");
            }
        }
    }
}

