using Arbiter.Core;
using Arbiter.MSBuild;
using Autofac;
using Moq;
using Serilog;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;

namespace Arbiter.Tests.Helpers
{
    public class ContainerHelper
    {
        public static ContainerHelper ConfigureContainer()
        {
            return new ContainerHelper();
        }

        private readonly ContainerBuilder _builder;

        public ContainerHelper()
        {
            _builder = new ContainerBuilder();
            var assemblies = new[] { typeof(ArbiterRootCommand).Assembly, typeof(ArbiterMSBuildLocator).Assembly };
            _builder.RegisterAssemblyTypes(assemblies)
                .PublicOnly().Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
                .AsSelf().AsImplementedInterfaces(); _builder.RegisterInstance(new Mock<ILogger>().Object);

            _builder.RegisterType<TestConsole>().As<IConsole>();
            _builder.RegisterType<MockFileSystem>().As<IFileSystem>();
        }

        public ContainerHelper WithRealFileSystem()
        {
            _builder.RegisterType<FileSystem>().As<IFileSystem>();

            return this;
        }

        public ContainerHelper WithRegistrations(Action<ContainerBuilder> registrationActions)
        {
            registrationActions.Invoke(_builder);

            return this;
        }

        public IContainer Build()
        {
            return _builder.Build();
        }
    }
}
