using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Data.Models
{
    public class InstanceFilters : Request
    {
        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
        public long? Prototype { get; set; }
        public int? Version { get; set; }
        public long? ID { get; set; }
        public string Description { get; set; }
        public int? Year { get; set; }
        public int? Number { get; set; }
        public string TagCode { get; set; }
        public string TagValue { get; set; }
        public string State { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string ExceptionState { get; set; }
        public string ExceptionDescription { get; set; }
        public int? LifeCycleFrom { get; set; }
        public int? LifeCycleTo { get; set; }

        public long? CompanyId { get; set; }
        public string User {  get; set; }
    }
}
