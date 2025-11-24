using System;
using System.Runtime.Serialization;

namespace CheckListManager.Common.Core.Exceptions
{
    [Serializable]
    public class DatabaseException : HandledException
    {
        public DatabaseException(string message, Exception innerExceptio = null) : base(message, innerExceptio)
        {
        }

        public int? Position { get; set; }
    }
}