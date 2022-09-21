using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal.InstancesAnalysis
{
    public class InstanceListItem
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long Company { get; set; }
        public string PrototypeName { get; set; }
        public int Version { get; set; }
        public int Year { get; set; }
        public int Number { get; set; }
        public string InstanceState { get; set; }
    }
}