using System;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class ElementException : Exception
    {
        public ElementException(string message, Element element) : base (message)
        {
            Id = (element as Item)?.Id;
            Description = (element as Item)?.Description ?? (element as Page)?.Description;
        }

        public int? Id { get; }

        public string Description { get; }
    }
}
