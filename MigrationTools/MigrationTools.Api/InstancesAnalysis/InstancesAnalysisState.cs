using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Instances.Logic.Models
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

        public List<Error> Errors { get; set; }

        public InstancesAnalysisState()
        {

        }

        public dynamic Dump()
        {
            var Running = State == "Started";

            var EstimatedMilliseconds = !InitialTime.HasValue || CurrentRecordIndex == 0 ? default(double?) : (DateTime.Now - InitialTime.Value).TotalMilliseconds / CurrentRecordIndex * (RecordCount - CurrentRecordIndex);
            var MissingTime = Running && EstimatedMilliseconds.HasValue ? TimeSpan.FromMilliseconds(EstimatedMilliseconds.Value) : default(TimeSpan?);

            return new
            {
                status = State,
                running = Running,
                initialTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", InitialTime),
                finalTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", Running ? default(DateTime?) : FinalTime),
                recordCount = RecordCount,
                recordIndex = CurrentRecordIndex,
                ok = Ok,
                ko = Ko,
                elapsedTime = string.Format("{0:hh\\:mm\\:ss}", InitialTime.HasValue ? (FinalTime ?? DateTime.Now) - InitialTime : default(TimeSpan?)),
                missingTime = string.Format("{0:hh\\:mm\\:ss}", MissingTime),
                errors = Errors.Any() ? Errors : null,
            };
        }

        public class Error
        {
            public string Key { get; set; }
            public string Message { get; set; }
        }
    }
}