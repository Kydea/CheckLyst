using System;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class ElementException : Exception
    {
        public ElementException(string message, ICheckListElement element) : base (message)
        {
            Id = (element as ICheckListItem)?.Id;
	        Description = (element as ICheckListItem)?.Description ?? (element as ICheckListPage)?.Description;
        }

        public int? Id { get; }

        public string Description { get; }
    }
}
