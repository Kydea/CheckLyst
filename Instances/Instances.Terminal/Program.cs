using Topshelf;

namespace CheckListManager.Instances.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HostFactory.Run(x =>
			{
				x.Service(settings => new InstancesWindowsService(new InstancesTerminalConfig()));

				x.SetDisplayName("CheckListManager - Instances Service");
				x.SetDescription("CheckListManager - Instances Service");
				x.SetServiceName("CheckListManager - Instances Service");

				x.EnableShutdown();

				x.StartAutomatically();

				x.UseNLog();
			});
		}
	}
}
