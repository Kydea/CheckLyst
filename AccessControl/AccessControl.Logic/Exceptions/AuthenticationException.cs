using System;

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
	}
}
