using Autofac;
using AutoMapper;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Terminal
{
	public class InstancesTerminalConfig : TerminalConfig
	{
		protected override void OnCreateBuilder(ContainerBuilder builder)
		{
			base.OnCreateBuilder(builder);

			RegisterChecklistItem<Field, ICheckListField>(builder);
			RegisterChecklistItem<Group, ICheckListGroup>(builder);
			RegisterChecklistItem<Iteration, ICheckListIteration>(builder);
			RegisterChecklistItem<Page, ICheckListPage>(builder);
			RegisterChecklistItem<Signature, ICheckListSignature>(builder);
			RegisterChecklistItem<Text, ICheckListText>(builder);

			builder.RegisterType<CheckListJsonConverter>()
				.AsSelf()
				.InstancePerDependency();
		}

		private static void RegisterChecklistItem<TClass, TInterface>(ContainerBuilder builder) where TClass : TInterface
		{
			builder.RegisterType<TClass>()
				.As<TInterface>()
				.AsSelf()
				.InstancePerDependency();
		}

		protected override void OnCreateMapper(IMapperConfigurationExpression mapperConfig)
		{
			base.OnCreateMapper(mapperConfig);

			mapperConfig.CreateMap<TrailMessage, Records>();
		}
	}
}
