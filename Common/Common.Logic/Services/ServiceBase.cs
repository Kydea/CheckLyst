using System;
using System.Collections.Generic;
using System.Web.Http;
using Common.Core.Services;

namespace Common.Logic.Services
{
	public abstract class ServiceBase : IService
	{
		protected virtual object GetService(Type type)
		{
			return GlobalConfiguration.Configuration.DependencyResolver.GetService(type);
		}

		protected virtual IEnumerable<object> GetServices(Type type)
		{
			return GlobalConfiguration.Configuration.DependencyResolver.GetServices(type);
		}

		protected virtual T GetService<T>()
		{
			return (T)GetService(typeof(T));
		}

		protected virtual IEnumerable<T> GetServices<T>()
		{
			return (IEnumerable<T>)GetServices(typeof(T));
		}
	}
}
