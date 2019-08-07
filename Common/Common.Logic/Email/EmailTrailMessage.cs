namespace CheckListManager.Common.Logic.Email
{
    public class EmailTrailMessage
    {
        public EmailTrailMessage(string user, string checkList, EmailTrailInformation information)
        {
            User = user;
            CheckList = checkList;
            Information = information ?? new EmailTrailInformation();
        }
        
        public string User { get; }
        public string CheckList { get; }
        public EmailTrailInformation Information { get; }

        public class EmailTrailInformation
        {
            public string Subject { get; set; }
            public string[] Recipients { get; set; }
            public string Id { get; set; }
        }
    }
}