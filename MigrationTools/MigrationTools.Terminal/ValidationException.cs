using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal
{
    public class ValidationException: Exception
    {
        public ValidationException(List<string> Messages)
        {
            this.Messages = Messages;
        }

        public ValidationException(List<Tuple<string, int>> MessagesWithOffset)
        {
            this.MessagesWithOffset = MessagesWithOffset;
        }

        public List<string> Messages { get; }

        public List<Tuple<string, int>> MessagesWithOffset { get; }
    }
}