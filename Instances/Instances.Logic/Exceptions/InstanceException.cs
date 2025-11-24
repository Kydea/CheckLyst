using CheckListManager.Common.Core.Exceptions;
using System;

namespace CheckListManager.Instances.Logic.Exceptions
{
	[Serializable]
	public class InstanceException : HandledException
	{
		public InstanceException(string message) : base(message)
		{
		}

		public InstanceException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
