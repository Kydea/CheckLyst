using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Matematici.DataSource
{
    public class Engine
    {
        public static Engine Instance { get; private set; }

        public static void Initialize()
        {
            Instance = new Engine();
        }

        private Engine()
        {
            foreach (var Company in Settings.Provider.Values.Companies)
                Companies.Add(new Company(Company.Name, Company.PrototypesFolder));
        }

        public List<Company> Companies { get; } = new List<Company>();
   }
}