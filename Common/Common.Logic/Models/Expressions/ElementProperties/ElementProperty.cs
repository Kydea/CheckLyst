using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Common.Logic.Models.Expressions.ElementProperties
{
    public class ElementProperty : Symbol
    {
        public enum ElementProperties { Valid }

        private readonly ElementProperties Property;

        protected ElementProperty()
        {
        }

        public ElementProperty(ElementProperties Property)
        {
            this.Property = Property;
        }

        public string Code { get; set; }

        protected virtual void Check(ICheckListElement Element, List<string> IteratorStack)
        {
            if (Property == ElementProperties.Valid)
                DataType = DataType.Boolean;
        }

        public static ICheckListElement Check(ICheckList Prototype, List<string> IteratorStack, string Code, int Position)
        {
            if (!Prototype.Elements.ContainsKey(Code))
                throw new ParseException($"Element with code {Code} not found", Position);

            var Element = Prototype.Elements[Code];
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

        public override void Check(ICheckList Prototype, List<string> IteratorStack)
        {
            var Element = Check(Prototype, IteratorStack, Code, Position);
            Check(Element, IteratorStack);
        }

        protected virtual TypedValue GetValue(ICheckListElement Element, string TimeZone)
        {
            return new TypedValue(DataType.Boolean, Element.Valid);
        }

        public override TypedValue GetValue(IElementsBranch ElementsBranch, string TimeZone)
        {
            var Element = ElementsBranch.Elements[Code];
            return GetValue(Element, TimeZone);
        }

        public override string ToString()
        {
            return $"{Code}.Valid";
        }
    }
}
