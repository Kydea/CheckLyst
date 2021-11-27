namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckListAttachmentBase : ICheckListItem
    {
        string Key { get; set; }
        string Target { get; set; }
        ICheckListAttachmentBase GetTarget();
    }
}