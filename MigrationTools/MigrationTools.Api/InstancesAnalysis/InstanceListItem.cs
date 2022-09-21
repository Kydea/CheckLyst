using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Instances.Logic.Models
{
    public class InstanceListItem
    {
        public string Code { get; set; }
        public long Company { get; set; }
        public string PrototypeName { get; set; }
        public int Version { get; set; }
        public int Year { get; set; }
        public int Number { get; set; }
        public string InstanceState { get; set; }
    }
}