using System;
using Autofac;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public abstract class ServiceBase : IService
	{
		private readonly IComponentContext _context;

		protected ServiceBase(IComponentContext context)
		{
			_context = context;
		}

		protected virtual object GetDependency(Type type)
			=> _context.Resolve(type);

		protected virtual T GetDependency<T>()
			=> _context.Resolve<T>();
	}
}
