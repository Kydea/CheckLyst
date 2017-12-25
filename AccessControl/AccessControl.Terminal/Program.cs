using Topshelf;

namespace CheckListManager.Trail.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new AccessControlWindowsService(new AccessControlTerminalConfig()));

				x.SetDisplayName("CheckListManager - Access Control Service");
				x.SetDescription("CheckListManager - Access Control Service");
				x.SetServiceName("CheckListManager - Access Control Service");

				x.EnableShutdown();

				x.StartAutomatically();

				x.UseNLog();
			});
		}
	}
}
