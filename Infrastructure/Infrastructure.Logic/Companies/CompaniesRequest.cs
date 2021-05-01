using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Companies
{
    public class CompaniesRequest : Request
    {
        public long? ID { get; set; }
        public string Name { get; set; }
        public string TimeZone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Reason { get; set; }
    }
}
