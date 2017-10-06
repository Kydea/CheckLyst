using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.DataEngine
{
    public class ValidationMessage
    {
        public const string
            LEVEL_ERROR = "ERROR",
            LEVEL_WARNING = "WARNING",
            LEVEL_INFORMATION = "INFO";

        public string Level { get; set; }

        public string Message { get; set; }
    }
}
