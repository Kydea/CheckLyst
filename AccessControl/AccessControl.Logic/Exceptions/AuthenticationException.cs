using System;
using System.Runtime.Serialization;

namespace CheckListManager.AccessControl.Logic.Exceptions
{
	[Serializable]
	public class AuthenticationException : Exception
	{
		public int ErrorCode { get; }

		public AuthenticationException(int errorCode, string message) : base(message)
		{
			ErrorCode = errorCode;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("ErrorCode", ErrorCode);
		}
	}
}
