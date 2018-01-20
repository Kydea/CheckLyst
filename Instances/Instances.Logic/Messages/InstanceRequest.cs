namespace CheckListManager.Instances.Logic.Messages
{
	public class InstanceRequest
	{
		public string Code { get; set; }
        public string Note { get; set; }
        public long? ID { get; set; }
        public long? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string UserCode { get; set; }
        public string TimeZone { get; set; }
    }
}
