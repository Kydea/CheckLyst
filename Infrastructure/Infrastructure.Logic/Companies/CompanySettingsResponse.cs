namespace CheckListManager.Infrastructure.Logic.Companies
{
    public class CompanySettingsResponse
    {
        public bool RequireUserReason { get; set; }
        public string WebApplicationUrl { get; set; }
        public int InactivityTimeoutSeconds { get; set; }
    }
}