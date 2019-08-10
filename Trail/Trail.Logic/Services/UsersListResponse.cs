using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CheckListManager.Trail.Logic.Services
{
    public class UsersListResponse
    {
        public long Company { get; set; }
        public long Id { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public string Action { get; set; }
        public string User { get; set; }
        public string AffectedUser { get; set; }
        public string Information { get; set; }

        [JsonIgnore]
        public UserInformation Details { get; set; }

        public class UserInformation
        {
            public string Code { get; set; }
            public string Email { get; set; }
            public string NameAndSurname { get; set; }
            
            public bool? Enabled { get; set; }

            public List<Dictionary<string, string>> Functions { get; set; } = new List<Dictionary<string, string>>();
        }
    }
}