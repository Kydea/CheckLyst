using System.Web.Http;
using Autofac;
using CheckListManager.Common.Logic.Api.Configuration;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Instances.Logic.Models.CheckLists;
using MultipartDataMediaFormatter;
using MultipartDataMediaFormatter.Infrastructure;
using Owin;

namespace CheckListManager.Instances.Api
{
    public class WebApiConfig : ApiConfig
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

            builder.RegisterType<TypedValueJsonConverter>()
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

	    protected override void OnConfigureWebApi(HttpConfiguration configuration)
	    {
		    base.OnConfigureWebApi(configuration);

		    configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new CheckListJsonConverter(Container));
            configuration.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter(new MultipartFormatterSettings()));
	    }

	    #region Static

		public static void Setup(IAppBuilder app)
	    {
		    var apiConfig = new WebApiConfig
		    {
			    LoadAssemblies = true
		    };

		    apiConfig.Configure(app);
	    }

		#endregion
	}
}