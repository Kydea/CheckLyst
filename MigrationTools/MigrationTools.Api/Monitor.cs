using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CheckListManager.MigrationTools.Api
{
    public class Monitor
    {
        private long CompanyID { get; set; }
        private string CompanyDescription { get; set; }
        private bool Running { get; set; }
        public bool Aborted { get; private set; }
        private DateTime? InitialTime { get; set; }
        private DateTime? IterationTime { get; set; }
        private DateTime? FinalTime { get; set; }
        private int RecordCount { get; set; }
        private int CurrentRecordIndex { get; set; }
        public Dictionary<string, CountRecord> Counts { get; } = new Dictionary<string, CountRecord>();
        public Dictionary<string, List<string>> Errors { get; } = new Dictionary<string, List<string>>();

        public Monitor()
        {

        }

        public Monitor(long CompanyID, string CompanyDescription)
        {
            this.CompanyID = CompanyID;
            this.CompanyDescription = CompanyDescription;
        }

        public void Start(int RecordCount)
        {
            if (Running) throw new InvalidOperationException();
            InitialTime = DateTime.Now;
            Running = true;
            Aborted = false;
            this.RecordCount = RecordCount;
            CurrentRecordIndex = 0;
        }

        public void Inc()
        {
            IterationTime = IterationTime ?? DateTime.Now;
            ++CurrentRecordIndex;
        }

        public void Stop()
        {
            Running = false;
            IterationTime = null;
            FinalTime = DateTime.Now;
        }

        public dynamic Dump()
        {
            var EstimatedMilliseconds = !IterationTime.HasValue || CurrentRecordIndex == 0 ? default(double?) : (DateTime.Now - IterationTime.Value).TotalMilliseconds / CurrentRecordIndex * (RecordCount - CurrentRecordIndex);
            var MissingTime = Running && EstimatedMilliseconds.HasValue ? TimeSpan.FromMilliseconds(EstimatedMilliseconds.Value) : default(TimeSpan?);

            return new
            {
                company = CompanyID,
                companyDescription = $"{CompanyID} - {CompanyDescription}",
                status = Running && Aborted ? "Aborting" : Running ? (CurrentRecordIndex == 0 ? "Starting" : "Running") : Aborted ? "Aborted" : "Stopped",
                running = Running,
                aborted = Aborted,
                initialTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", InitialTime),
                finalTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", Running ? default(DateTime?) : FinalTime),
                recordCount = RecordCount,
                recordIndex = CurrentRecordIndex,
                elapsedTime = string.Format("{0:hh\\:mm\\:ss}", InitialTime.HasValue ? (FinalTime ?? DateTime.Now) - InitialTime : default(TimeSpan?)),
                missingTime = string.Format("{0:hh\\:mm\\:ss}", MissingTime),
                counts = Counts.Any() ? Counts : null,
                errors = Errors.Any() ? Errors : null,
            };
        }

        public void Abort() => Aborted = true;

        public class CountRecord
        {
            public int ko { get; set; }
            public int ok { get; set; }
        }
    }
}