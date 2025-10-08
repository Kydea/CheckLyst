using RedLockNet;
using System;

namespace CheckListManager.Common.Logic.Cache
{
	public class LockObject : IDisposable
	{
		private readonly IRedLock _lockObject;

		public LockObject(IRedLock lockObject)
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
			if (disposing) _lockObject?.Dispose();
		}

		public bool IsAcquired => _lockObject?.IsAcquired ?? false;
	}
}
