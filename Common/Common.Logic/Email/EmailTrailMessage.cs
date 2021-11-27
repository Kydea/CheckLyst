namespace CheckListManager.Common.Logic.Email
{
    public class EmailTrailMessage
    {
        public EmailTrailMessage(string user, string checkListCode, long? checkListID, EmailTrailInformation information)
        {
            User = user;
            CheckListCode = checkListCode;
            CheckListID = checkListID;
            Information = information ?? new EmailTrailInformation();
        }
        
        public string User { get; }
        public string CheckListCode { get; }
        public long? CheckListID { get; }
        public EmailTrailInformation Information { get; }

        public class EmailTrailInformation
        {
            public string Subject { get; set; }
            public string[] Recipients { get; set; }
            public string Id { get; set; }
        }
    }
}