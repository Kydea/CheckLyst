using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class InstanceResponse
	{
		public long? Id { get; set; }
		public string Code { get; set; }

		public Instance CheckList { get; set; }
	}
}
