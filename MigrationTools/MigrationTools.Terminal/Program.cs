using Topshelf;

namespace CheckListManager.MigrationTools.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new MigrationToolsWindowsService(new MigrationToolsTerminalConfig()));

				x.SetDisplayName("CheckListManager - Prototypes Service");
				x.SetDescription("CheckListManager - Prototypes Service");
				x.SetServiceName("CheckListManager - Prototypes Service");

				x.EnableShutdown();

				x.StartAutomatically();

				x.UseNLog();
			});
		}
	}
}
