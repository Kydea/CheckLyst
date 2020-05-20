namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListAttachment : ICheckListAttachmentBase, ICheckListEditableItem
    {
		string FileType { get; set; }
	}
}