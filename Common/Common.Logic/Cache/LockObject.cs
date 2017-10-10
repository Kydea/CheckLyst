using System;
using RedLock;

namespace CheckListManager.Common.Logic.Cache
{
	public class LockObject : IDisposable
	{
		private readonly IRedisLock _lockObject;

		public LockObject(IRedisLock lockObject)
		{
			_lockObject = lockObject;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// ReSharper disable once VirtualMemberNeverOverridden.Global
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_lockObject?.Dispose();
			}
		}

		public bool IsAcquired => _lockObject?.IsAcquired ?? false;
	}
}
