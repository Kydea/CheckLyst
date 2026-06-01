namespace CheckListManager.AccessControl.Data.Models

{
    public class SummaryRecord
    {
        public long CompanyID { get; set; }
        public string CompanyDescription { get; set; }
        public long TotalUsers { get; set; }
        public long ActiveUsers { get; set; }
    }
}
