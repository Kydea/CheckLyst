using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal
{
    public class SystemException: Exception
    {
        public SystemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}