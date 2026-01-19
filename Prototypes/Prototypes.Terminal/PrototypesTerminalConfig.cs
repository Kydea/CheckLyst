using Autofac;
using AutoMapper;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Prototypes.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Terminal
{
	public class PrototypesTerminalConfig : TerminalConfig
	{
        protected override void OnCreateBuilder(ContainerBuilder builder)
        {
            base.OnCreateBuilder(builder);

            RegisterChecklistItem<Field, ICheckListField>(builder);
            RegisterChecklistItem<Group, ICheckListGroup>(builder);
            RegisterChecklistItem<Iteration, ICheckListIteration>(builder);
            RegisterChecklistItem<Page, ICheckListPage>(builder);
            RegisterChecklistItem<Signature, ICheckListSignature>(builder);
            RegisterChecklistItem<InstanceLink, ICheckListInstanceLink>(builder);
            RegisterChecklistItem<Text, ICheckListText>(builder);
            RegisterChecklistItem<Note, ICheckListNote>(builder);
            RegisterChecklistItem<Link, ICheckListLink>(builder);
            RegisterChecklistItem<Image, ICheckListImage>(builder);
            RegisterChecklistItem<Attachment, ICheckListAttachment>(builder);

            builder.RegisterType<CheckListJsonConverter>()
                .AsSelf()
                .As<CustomJsonConverter>()
                .InstancePerDependency();

            builder.RegisterType<ExternalSourceJsonConverter>()
                .AsSelf()
                .As<CustomJsonConverter>()
                .InstancePerDependency();
        }

        private static void RegisterChecklistItem<TClass, TInterface>(ContainerBuilder builder) where TClass : TInterface
        {
            builder.RegisterType<TClass>()
                .As<TInterface>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
