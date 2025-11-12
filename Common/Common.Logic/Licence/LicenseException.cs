using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Licence
{
    public class LicenseException: Exception
    {
        public LicenseException(string message) : base(message)
        {
        }
    }
}
