using Arbiter.Core;
using Autofac;

namespace Aribter.Core.Tests
{
    public static class ContainerHelper
    {
        public static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            var assemblies = new[] { typeof(CommandBuilder).Assembly };
            builder.RegisterAssemblyTypes(assemblies).PublicOnly().AsSelf().AsImplementedInterfaces();
            return builder.Build();
        }
    }
}
