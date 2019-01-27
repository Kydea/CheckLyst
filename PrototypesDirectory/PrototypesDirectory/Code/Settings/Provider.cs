using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Matematici.Settings
{
    public class Provider
    {
        public static Provider Values { get; set; }

        public List<Company> Companies { get; set; }
        public bool DevelopmentMode { get; set; }
        public int SessionTimeout { get; set; }

        public static void TestPath(string Path)
        {
            try
            {
                var FileName = Path + "/Touch.log";
                if (File.Exists(FileName))
                    File.Delete(FileName);

                using (var Touch = File.Create(FileName))
                    Touch.Write(new byte[] { 0 }, 0, 1);
            }
            catch (Exception)
            {
                throw new Exception("Error in configuration file. The application cannot write on folder " + Path + ".");
            }
        }

        public void Setup()
        {

            if (SessionTimeout < 60)
                throw new Exception("Error in configuration file. Activity Timeout and Session Timeout must be greater or equal than 60 seconds.");

            int Index = 0;
            foreach (var Company in Companies)
            {
                Company.Index = Index++;
                Company.Check();
            }
        }
    }
}
