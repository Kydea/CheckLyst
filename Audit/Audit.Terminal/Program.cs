using Topshelf;

namespace CheckListManager.Audit.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new AuditWindowsService(new AuditTerminalConfig()));

				x.SetDisplayName("CheckListManager - Audit Service");
				x.SetDescription("CheckListManager - Audit Service");
				x.SetServiceName("CheckListManager - Audit Service");
				x.SetInstanceName("CheckListManager - Audit Service");

				x.EnableShutdown();

				x.StartAutomatically();

				x.UseNLog();
			});
		}
	}
}
