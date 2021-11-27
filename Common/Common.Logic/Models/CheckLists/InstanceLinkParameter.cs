using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public class InstanceLinkParameter
    {
        public string Name { get; set; }
        public string ExpressionCode { get; set; }
        public string ElementCode { get; set; }
        public string Value { get; set; }
        public string Error { get; set; }

        public void BuildDependencies(ICheckList CheckList, DependencyNode InstanceLink)
        {
            Error = null;
            if (ExpressionCode != null)
            {
                ElementCode = null;
                if (!CheckList.Expressions.TryGetValue(ExpressionCode, out var Expression))
                    Error = $"Expression {ExpressionCode} not found";
                else if (Expression.DataType == DataType.Array)
                    Error = $"Expression {ExpressionCode} is an array";
                else
                    InstanceLink.AddDependingRelation(DependencyRelation.Properties.Parameter, Expression, DependencyRelation.Properties.Value);

            }
            else if (ElementCode != null)
            {
                if (!CheckList.Elements.TryGetValue(ElementCode, out var Element))
                    Error = $"Element {ElementCode} not found";
                else if (!(Element is ICheckListField Field))
                    Error = $"Element {ElementCode} is not a field";
                else
                    InstanceLink.AddDependingRelation(DependencyRelation.Properties.Parameter, (DependencyNode)Field, DependencyRelation.Properties.Value);
            }
        }
    }
}