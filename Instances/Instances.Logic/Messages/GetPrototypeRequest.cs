namespace CheckListManager.Instances.Logic.Messages
{
	public class GetPrototypeRequest
	{
		public long Id { get; set; }
		public int Version { get; set; }
        public bool InstanceCreation { get; } = true;
	}
}
