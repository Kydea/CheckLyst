using System;
using System.Threading;
using Autofac;
using CheckListManager.Common.Logic.Email;
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

				var result = OnStart(hostControl);
				Notify($"{GetType().Name} START", $"{GetType().Name} Started at {DateTime.UtcNow}");
				return result;
			}
			catch (Exception exception)
			{
				Notify($"{GetType().Name} Failed to START", exception);
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
				var result = OnStop(hostControl);
				Notify($"{GetType().Name} STOP", $"{GetType().Name} Stopped at {DateTime.UtcNow}");
				return result;
			}
			catch (Exception exception)
			{
				Notify($"{GetType().Name} Failed to STOP", exception);
				Logger.Fatal(exception);
				return false;
			}
			finally
			{
				Container?.Dispose();
			}
		}

		private void Notify(string subject, string content)
		{
			var service = Container.Resolve<IEmailServiceBase>();
			service.LogInfoAsync(subject, content);
		}

		private void Notify(string subject, Exception exception)
		{
			var service = Container.Resolve<IEmailServiceBase>();
			service.LogErrorAsync(exception, subject);
		}

		protected abstract bool OnStart(HostControl hostControl);
		protected abstract bool OnStop(HostControl hostControl);
	}
}
