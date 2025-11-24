using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Exceptions
{
    public class HandledException : Exception
    {
        public HandledException(string message) : base(message) { }

        public HandledException(string message, Exception innerException) : base(message, innerException) { }
    }
}
