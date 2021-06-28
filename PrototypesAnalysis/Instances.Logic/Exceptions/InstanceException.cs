using System;
using System.Runtime.Serialization;

namespace CheckListManager.Instances.Logic.Exceptions
{
	[Serializable]
	public class InstanceException : Exception
	{
		public InstanceException()
		{
		}

		public InstanceException(string message) : base(message)
		{
		}

		public InstanceException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InstanceException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
