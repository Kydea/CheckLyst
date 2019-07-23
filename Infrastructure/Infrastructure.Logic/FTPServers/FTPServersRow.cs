using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.FTPServers
{
    public class FTPServersRow
    {
        public long? ID { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Folder { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Passive { get; set; }
        public bool SSL { get; set; }
        public bool PrototypesDefault { get; set; }
        public bool InstancesDefault { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
