using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.MigrationTools.Api.PrototypesAnalysis
{
    public class ValidationException: Exception
    {
        public ValidationException(List<string> Messages)
        {
            this.Messages = Messages;
        }

        public List<string> Messages { get; }
    }
}