using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;

namespace CheckListManager.Trail.Terminal
{
	public class AccessControlWindowsService : TimerWindowsService
	{
		public AccessControlWindowsService(TerminalConfig config) : base(config)
		{
            Add(new ComputedSchedulerStrategy(RunAsync, new[] { 0 }, new[] { 0 }) { RunAtStartup = true });
        }

		protected async Task RunAsync(CancellationToken cancellationToken)
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var userService = scope.Resolve<IUserService>();
                var commonRepository = scope.Resolve<ICommonRepository>();
                foreach (var company in await commonRepository.GetCompaniesAsync())
                    if (company.ID > 1)
                    {
                        await userService.CheckInactiveUsers(company.ID, cancellationToken);
                        await userService.CheckUnchangedPins(company.ID, cancellationToken);
                    }
			}
		}
	}
}
