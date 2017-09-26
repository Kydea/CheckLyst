using Autofac;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Terminal.Configuration;

namespace CheckListManager.Audit.Terminal
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var config = new TerminalConfig();
			var builder = config.CreateBuilder();

			var container = builder.Build();

			using (var scope = container.BeginLifetimeScope())
			{
				var queue = scope.Resolve<IQueueService>();
			}
		}
	}
}
