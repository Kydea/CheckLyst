using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CheckListManager.Common.Data.Exceptions
{
	[Serializable]
	public class DatabaseAggregateException : AggregateException
	{
		public DatabaseAggregateException()
		{
		}

		public DatabaseAggregateException(string message) : base(message)
		{
		}

		public DatabaseAggregateException(IEnumerable<Exception> innerExceptions) : base(innerExceptions)
		{
		}

		public DatabaseAggregateException(params Exception[] innerExceptions) : base(innerExceptions)
		{
		}

		public DatabaseAggregateException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public DatabaseAggregateException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
		{
		}

		public DatabaseAggregateException(string message, params Exception[] innerExceptions) : base(message, innerExceptions)
		{
		}

		protected DatabaseAggregateException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
