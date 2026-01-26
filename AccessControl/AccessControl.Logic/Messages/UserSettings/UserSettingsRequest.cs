using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.UserSettings
{
	public class UserSettingsRequest
    {
        public string Setting { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }
}
