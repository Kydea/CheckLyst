namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListAttachment : ICheckListAttachmentBase
	{
		string FileType { get; set; }
	}
}