using System;
using Autofac;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public class SingletonServiceBase : ISingletonService
	{
		private readonly IComponentContext _context;

		protected SingletonServiceBase(IComponentContext context)
		{
			_context = context;
		}

		protected virtual object GetDependency(Type type)
			=> _context.Resolve(type);

		protected virtual T GetDependency<T>()
			=> _context.Resolve<T>();
	}
}
