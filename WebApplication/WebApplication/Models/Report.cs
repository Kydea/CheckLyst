using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.WebApplication.Models
{
    public class Report
    {
        public long? ID { get; set; }
        public long? Run { get; set; }
        public bool NewVersion { get; set; }
    }
}