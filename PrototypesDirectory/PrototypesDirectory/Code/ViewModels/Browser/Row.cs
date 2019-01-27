using Matematici.DataEngine;
using Matematici.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Matematici.ViewModels.Browser
{
    public class Row
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string PrototypeType { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public string FileName { get; set; }
    }
}
