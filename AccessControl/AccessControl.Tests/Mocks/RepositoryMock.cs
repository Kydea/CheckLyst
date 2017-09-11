using System;
using System.Collections.Generic;

namespace CheckListManager.AccessControl.Tests.Mocks
{
	public abstract class RepositoryMock<T> : IDisposable
	{
		protected List<T> Data { get; } = new List<T>();

		public virtual void Dispose()
		{
		}
	}
}
