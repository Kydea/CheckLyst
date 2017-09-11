using System;
using System.Collections.Generic;
using System.Web.Http;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	//TODO remove dependency to GlobalConfiguration.Configuration
	public class SingletonServiceBase : ISingletonService
	{
		protected virtual object GetDependency(Type type)
		{
			return GlobalConfiguration.Configuration.DependencyResolver.GetService(type);
		}

		protected virtual IEnumerable<object> GetDependencies(Type type)
		{
			return GlobalConfiguration.Configuration.DependencyResolver.GetServices(type);
		}

		protected virtual T GetDependency<T>()
		{
			return (T)GetDependency(typeof(T));
		}

		protected virtual IEnumerable<T> GetDependencies<T>()
		{
			return (IEnumerable<T>)GetDependencies(typeof(T));
		}
	}
}
