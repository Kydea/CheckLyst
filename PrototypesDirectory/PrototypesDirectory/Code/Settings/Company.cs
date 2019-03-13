using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matematici.Settings
{
    public class Company
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string PrototypesFolder { get; set; }
        public string LDAPServer { get; set; }
        public string LDAPQuery { get; set; }
        public string LDAPDefaultDomain { get; set; }

        public void Check()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PrototypesFolder)) throw new Exception("Error in configuration file. Invalid value for PrototypesFolder");
                if (string.IsNullOrWhiteSpace(LDAPQuery)) throw new Exception("Error in configuration file. Invalid value for LDAPQuery");
                if (!PrototypesFolder.EndsWith("\\") & !PrototypesFolder.EndsWith("/"))
                    PrototypesFolder += "\\";
                Provider.TestPath(PrototypesFolder);
            }
            catch (Exception Exception)
            {
                throw new Exception($"Configuration error in company {Name ?? "(no name)"}", Exception);
            }
        }

    }
}