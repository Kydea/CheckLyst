using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Messages
{
	public class CreateInstanceResponse
	{
		public int? Id { get; set; }
		public string Code { get; set; }

		public Instance CheckList { get; set; }
	}
}
