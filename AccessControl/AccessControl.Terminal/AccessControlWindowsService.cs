using System.Threading.Tasks;
using Autofac;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;

namespace CheckListManager.Trail.Terminal
{
	public class AccessControlWindowsService : TimerWindowsService
	{
		public AccessControlWindowsService(TerminalConfig config) : base(config,
			new ComputedSchedulerStrategy(new[] { 0 }, new[] { 0 }) { RunAtStartup = true })
		{
		}

		protected override async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IUserService>();
				await service.CheckInactiveUsers(CancellationTokenSource.Token);
				await service.CheckUnchangedPins(CancellationTokenSource.Token);
			}
		}
	}
}
