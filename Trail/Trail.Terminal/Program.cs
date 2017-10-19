using Topshelf;

namespace CheckListManager.Trail.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new TrailWindowsService(new TrailTerminalConfig()));

				x.SetDisplayName("CheckListManager - Trail Service");
				x.SetDescription("CheckListManager - Trail Service");
				x.SetServiceName("CheckListManager - Trail Service");
				x.SetInstanceName("CheckListManager - Trail Service");

				x.EnableShutdown();

				x.StartAutomatically();

				x.UseNLog();
			});
		}
	}
}
