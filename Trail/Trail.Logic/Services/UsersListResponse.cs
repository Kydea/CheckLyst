using System;

namespace CheckListManager.Trail.Logic.Services
{
    public class UsersListResponse
    {
        public long Company { get; set; }
        public long Id { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public string Action { get; set; }
        public string ActionBy { get; set; }
        public string User { get; set; }
        public string Information { get; set; }

        public class UserInformation
        {
            public string Code { get; set; }
            public string Email { get; set; }
            public string NameAndSurname { get; set; }
        }
    }
}