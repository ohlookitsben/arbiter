using Arbiter.Core;
using Arbiter.MSBuild;
using Autofac;
using Moq;
using Serilog;
using System.Linq;
using System.Reflection;

namespace Arbiter.Tests
{
    public static class ContainerHelper
    {
        public static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            var assemblies = new[] { typeof(ArbiterRootCommand).Assembly, typeof(ArbiterMSBuildLocator).Assembly };
            builder.RegisterAssemblyTypes(assemblies)
                .PublicOnly().Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
                .AsSelf().AsImplementedInterfaces(); builder.RegisterInstance(new Mock<ILogger>().Object);
            return builder.Build();
        }
    }
}
