using AutoMapper;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Audit;
using CheckListManager.Common.Logic.Terminal.Configuration;

namespace CheckListManager.Audit.Terminal
{
	public class AuditTerminalConfig : TerminalConfig
	{
		protected override void OnCreateMapper(IMapperConfigurationExpression mapperConfig)
		{
			base.OnCreateMapper(mapperConfig);

			mapperConfig.CreateMap<AuditMessage, Records>();
		}
	}
}
