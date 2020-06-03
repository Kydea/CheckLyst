using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public abstract class ElementProperty : Symbol
    {
        public ElementProperty()
        {
        }

        public string Code { get; set; }

        protected string Property { get; set; }
        protected abstract DependencyRelation.Properties GetDependencyProperty(DependencyNode DependingElement);

        public override void BuildDependencies(ICheckList CheckList, Expression Expression)
        {
            if (!CheckList.Elements.ContainsKey(Code)) throw new ParseException($"Element with code {Code} not found", Position);
            var DependingElement = (DependencyNode)CheckList.Elements[Code];
            var Property = GetDependencyProperty(DependingElement);
            Expression.AddDependingRelation(DependencyRelation.Properties.Value, DependingElement, Property);
        }

        protected abstract void Check(ICheckListElement Element, List<string> IteratorStack);

        public static ICheckListElement Check(ICheckList CheckList, List<string> IteratorStack, string Code, int Position)
        {
            var Element = CheckList.Elements[Code];
            if (Element is ICheckListItem Item)
            {
                int Index = -1;
                ICheckListGroup Group = Item.Group;
                while (Group != null)
                {
                    if (Group.Iterator)
                    {
                        if (Group.Code == null)
                            throw new ParseException($"Element {Code} is contained by an iterator without a code", Position);

                        var GroupIndex = IteratorStack.IndexOf(Group.Code);

                        if (GroupIndex < 0)
                            throw new ParseException($"Element {Code} is contained by the group {Group.Code} which is not considered in the expression", Position);

                        if (Index >= 0 && GroupIndex != Index)
                            throw new ParseException($"The expression contains the group {IteratorStack[Index]} which does not contain the element {Code}", Position);

                        Index = GroupIndex - 1;
                    }
                    Group = Group.Group;
                }
            }

            return Element;
        }

        public override void Check(ICheckList CheckList, Expression Expression, List<string> IteratorStack)
        {
            var Element = Check(CheckList, IteratorStack, Code, Position);
            Check(Element, IteratorStack);
        }

        protected abstract TypedValue GetValue(ICheckListElement Element, string TimeZone);

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var Element = ElementsBranch.Elements[Code];
            return GetValue(Element, TimeZone);
        }

        public override string ToString()
            => $"{Code}.{Property}";
    }
}
