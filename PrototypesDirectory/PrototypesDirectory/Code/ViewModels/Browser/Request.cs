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
    public class Request : DataEngine.Request
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string PrototypeType { get; set; }
    }
}
