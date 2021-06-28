using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class SaveInstanceRequest
	{
		public string Code { get; set; }
		public Instance CheckList { get; set; }
	}
}
