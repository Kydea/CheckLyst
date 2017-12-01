using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.WebApplication.Models
{
    public class Prototype
    {
        public const string
            OPEN = "Open",
            CLONE = "Clone",
            CREATE = "Create",
            VERSION = "Version";

        public long Id { get; set; }

        public long Version { get; set; }

        public string Action { get; set; }
    }
}