using Topshelf;

namespace CheckListManager.Prototypes.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new PrototypesWindowsService(new PrototypesTerminalConfig()));

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
