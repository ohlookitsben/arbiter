using Arbiter.Core;
using Arbiter.MSBuild;
using Autofac;
using Moq;
using Serilog;

namespace Arbiter.Tests
{
    public static class ContainerHelper
    {
        public static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            var assemblies = new[] { typeof(CommandBuilder).Assembly, typeof(ArbiterMSBuildLocator).Assembly };
            builder.RegisterAssemblyTypes(assemblies).PublicOnly().AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(new Mock<ILogger>().Object);
            return builder.Build();
        }
    }
}
