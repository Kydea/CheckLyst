using System;
using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.Trail.Logic.Services
{
    public class UsersListRequest : Request
    {
        public long? CompanyId { get; set; }
        
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public DateTimeOffset? WholeTo => To?.AddDays(1);
        
        public string ActionBy { get; set; }
        
        public string User { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}