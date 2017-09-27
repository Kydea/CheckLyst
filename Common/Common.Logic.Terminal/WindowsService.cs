using System;
using System.Threading;
using Autofac;
using CheckListManager.Common.Logic.Terminal.Configuration;
using NLog;
using Topshelf;

namespace CheckListManager.Common.Logic.Terminal
{
	public abstract class WindowsService : ServiceControl
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly TerminalConfig _config;

		protected IContainer Container { get; private set; }
		protected CancellationTokenSource CancellationTokenSource { get; private set; }

		protected WindowsService(TerminalConfig config)
		{
			_config = config;
		}

		public bool Start(HostControl hostControl)
		{
			try
			{
				var builder = _config.CreateBuilder();
				Container = builder.Build();

				CancellationTokenSource = new CancellationTokenSource();

				return OnStart(hostControl);
			}
			catch (Exception exception)
			{
				Logger.Fatal(exception);
				return false;
			}
		}

		public bool Stop(HostControl hostControl)
		{
			try
			{
				CancellationTokenSource.Cancel(false);
			}
			catch (AggregateException exception)
			{
				Logger.Error(exception);
			}

			Thread.Sleep(3000); //Wait for operations to finish, if any

			try
			{
				Container?.Dispose();
				return OnStop(hostControl);
			}
			catch (Exception exception)
			{
				Logger.Fatal(exception);
				return false;
			}
		}

		protected abstract bool OnStart(HostControl hostControl);
		protected abstract bool OnStop(HostControl hostControl);
	}
}
