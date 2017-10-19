using AutoMapper;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.Terminal.Configuration;

namespace CheckListManager.Trail.Terminal
{
	public class TrailTerminalConfig : TerminalConfig
	{
		protected override void OnCreateMapper(IMapperConfigurationExpression mapperConfig)
		{
			base.OnCreateMapper(mapperConfig);

			mapperConfig.CreateMap<TrailMessage, Records>();
		}
	}
}
