using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.WebApplication.Models
{
    public class Instance
    {
        public long? Prototype { get; set; }
        public int? Version { get; set; }
        public string Code { get; set; }
        public string Roles { get; set; }
    }
}