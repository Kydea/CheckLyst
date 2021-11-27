using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Dependency
{
    public class DependencyRelation
    {
        public enum Properties { Value, Validity, Hidden, ReadOnly, Signature, Formula, CountDown, Iterations, AddIteration, RemoveIteration, MoveIteration, StateChange, Code, Parameter }

        public DependencyRelation()
        { }

        public DependencyRelation(Properties SourceProperty, DependencyNode DestinationNode, Properties DestinationProperty)
        {
            this.SourceProperty = SourceProperty;
            ClassName = DestinationNode.DependencyClassName;
            ObjectCode = DestinationNode.DependencyObjectCode;
            ObjectID = DestinationNode.DependencyObjectID;
            this.DestinationProperty = DestinationProperty;
        }

        public string ClassName { get; set; }
        public string ObjectCode { get; set; }
        public int? ObjectID { get; set; }
        [JsonProperty]
        public Properties SourceProperty { get; private set; }
        [JsonProperty]
        public Properties DestinationProperty { get; private set; }

        public override bool Equals(object o)
            => Equals(o as DependencyRelation);

        public bool Equals(DependencyRelation o)
            => o != null && o.ClassName == ClassName && o.ObjectCode == ObjectCode && o.ObjectID == ObjectID && SourceProperty == o.SourceProperty && o.DestinationProperty == DestinationProperty;

        public override int GetHashCode()
            => $"{SourceProperty}-{ClassName}-{ObjectCode ?? ObjectID?.ToString()}-{DestinationProperty}".GetHashCode();

        public override string ToString()
        {
            var ObjectID = ObjectCode == null && this.ObjectID.HasValue && ClassName == "page" ? this.ObjectID.Value + 1 : this.ObjectID;
            return $"{GetPropertyDescription(SourceProperty)} > {GetPropertyDescription(DestinationProperty)} of {ClassName} {ObjectCode ?? ObjectID?.ToString()}";
        }

        public string Description => ToString(); //used in the fronted

        public static string GetPropertyDescription(Properties Property)
            =>
                Property == Properties.Value ? "value" :
                Property == Properties.Validity ? "validity" :
                Property == Properties.Hidden ? "property hidden" :
                Property == Properties.ReadOnly ? "property readonly" :
                Property == Properties.Signature ? "signature" :
                Property == Properties.Formula ? "formula" :
                Property == Properties.CountDown ? "property countdown" :
                Property == Properties.Iterations ? "iterations" :
                Property == Properties.AddIteration ? "add iteration button" :
                Property == Properties.RemoveIteration ? "remove iteration button" :
                Property == Properties.MoveIteration ? "move iteration button" :
                Property == Properties.StateChange ? "state change" :
                Property == Properties.Code ? "code" :
                Property == Properties.Parameter ? "parameter" :
                throw new ArgumentException();


        public DependencyNode GetNode(ICheckList checkList)
        {
            switch (ClassName)
            {
                case "expression":
                    return checkList.Expressions[ObjectCode];
                case "element":
                    return (DependencyNode)checkList.Elements[ObjectCode];
                case "item":
                case "group":
                    return (DependencyNode)(ObjectID.HasValue ? checkList.Items[ObjectID.Value] : checkList.Elements[ObjectCode]);
                case "checklist":
                    return (DependencyNode)checkList;
                case "page":
                    return (DependencyNode)(ObjectID.HasValue ? checkList.Pages[ObjectID.Value] : checkList.Elements[ObjectCode]);
                case "external source":
                    return checkList.ExternalSources[ObjectCode];
                default:
                    throw new Exception($"Invalid class name {ClassName}");
            }
        }
    }
}

