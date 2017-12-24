using Topshelf;

namespace CheckListManager.Infrastructure.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new InfrastructureWindowsService(new InfrastructureTerminalConfig()));

				x.SetDisplayName("CheckListManager - Infrastructure Service");
				x.SetDescription("CheckListManager - Infrastructure Service");
				x.SetServiceName("CheckListManager - Infrastructure Service");

				x.EnableShutdown();

				x.StartAutomatically();

				x.UseNLog();
			});
		}
	}
}
