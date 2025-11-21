using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class UserRequest
    {
		public string Code { get; set; }
		public bool Enabled { get; set; }
		public string Password { get; set; }
        public string Password2 { get; set; }
        public string Description { get; set; }
		public string Email { get; set; }
		public string CorporateID { get; set; }
        public string AuthenticationCode { get; set; }
        public string TimeZone { get; set; }
        public Grant[] GeneralFunctions { get; set; }
        public Grant[] PrototypeFunctions { get; set; }
        public Grant[] InstanceFunctions { get; set; }
        public Grant[] Levels { get; set; }
        public List<string> Groups { get; set; }

        public string Reason { get; set; }
    }
}
