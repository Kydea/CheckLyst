using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal.InstancesAnalysis
{
    public class InstancesAnalysisState
    {
        public string State { get; set; }
        public int RecordCount { get; set; }
        public int CurrentRecordIndex { get; set; }
        public int Ok { get; set; }
        public int Ko { get; set; }
        private DateTime? InitialTime { get; set; }
        private DateTime? FinalTime { get; set; }
        public bool Started => State == "Started";

        public InstancesAnalysisState()
        {

        }
    }
}