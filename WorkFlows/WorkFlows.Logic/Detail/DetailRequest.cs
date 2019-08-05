namespace CheckListManager.WorkFlows.Logic.Detail
{
    public class DetailRequest
    {
        public long? ID { get; set; }
        public string Description { get; set; }
        public Step[] Steps { get; set; }

        public string Reason { get; set; }
    }
}
