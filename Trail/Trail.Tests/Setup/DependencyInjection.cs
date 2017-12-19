using Amazon.SQS;
using Autofac;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Trail.Api;
using SpecFlow.Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Trail.Tests.Setup
{
    public class DependencyInjection
    {
        [ScenarioDependencies]
        public static ContainerBuilder RegisterTypesWithSpecflowContainer()
        {
            var assemblies = LoadRefencedAssemblies();
            var config = new WebApiConfig { LoadAssemblies = false };
            var builder = config.CreateBuilder();

            builder.RegisterType<RelationalDatabaseEntities>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
                .AssignableTo<IRepository>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
                .AssignableTo<IService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(typeof(DependencyInjection).Assembly)
                .InstancePerLifetimeScope();

            builder.RegisterType<AmazonSQSClient>()
                .As<IAmazonSQS>()
                .AsSelf()
                .InstancePerLifetimeScope();

            return builder;
        }

        private static Assembly[] LoadRefencedAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();
            var availableAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "CheckListManager*.dll")
                .Select(AssemblyName.GetAssemblyName);
            var toLoad = availableAssemblies.Where(x => loadedAssemblies.All(a => a.FullName != x.FullName)).ToList();
            loadedAssemblies.AddRange(toLoad.Select(name => AppDomain.CurrentDomain.Load(name)));

            return loadedAssemblies.ToArray();
        }
    }
}
