using System;
using System.Runtime.Serialization;

namespace CheckListManager.Common.Data.Exceptions
{
    [Serializable]
    public class DatabaseException : Exception
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException, int? position = null) : base(message, innerException)
        {
            Position = position;
        }

        protected DatabaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DatabaseException(DatabaseError error, string message, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        public DatabaseError? Error { get; }

        public int? Position { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (Error.HasValue)
                info.AddValue("Error", Error.Value);
        }
    }
}
