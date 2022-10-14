using Topshelf;

namespace CheckListManager.Infrastructure.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new ReportsWindowsService(new InfrastructureTerminalConfig()));

				x.SetDisplayName("CheckListManager - Reports Service");
				x.SetDescription("CheckListManager - Reports Service");
				x.SetServiceName("CheckListManager - Reports Service");

				x.EnableShutdown();

				x.StartAutomatically();

				x.UseNLog();
			});
		}
	}
}
