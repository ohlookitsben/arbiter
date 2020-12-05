using Arbiter.Core;
using Arbiter.MSBuild;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using System;

namespace Arbiter
{
    public class Program
    {
        private readonly CommandBuilder _commandBuilder;

        private static int Main(string[] args)
        {
            args = new string[] { @"..\..\..\..\..\Arbiter.sln", "c6959c4", "a9ae776", "arbiter.nunit" };
            Program program;
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                // The correct MSBuild must be registered before types that reference it cause the assemblies to load.
                new ArbiterMSBuildLocator().RegisterDefaults();

                program = LoadProgram();
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal Exception:");
                Console.WriteLine(e);
                return -1;
            }

            return program.Run(args);
        }

        /// <summary>
        /// LoadProgram will reference code that requires MSBuild assemblies to be loaded so it must be in a separate method to the Registration call.
        /// </summary>
        /// <returns></returns>
        private static Program LoadProgram()
        {
            var builder = new ContainerBuilder();
            var assemblies = new[] { typeof(Program).Assembly, typeof(CommandBuilder).Assembly, typeof(MSBuildSolutionAnalyzer).Assembly };
            builder.RegisterAssemblyTypes(assemblies).PublicOnly().AsSelf().AsImplementedInterfaces();
            builder.RegisterLogger();

            var container = builder.Build();
            return container.Resolve<Program>();
        }

        public Program(CommandBuilder commandBuilder)
        {
            _commandBuilder = commandBuilder;
        }

        public int Run(string[] args)
        {
            var command = _commandBuilder.ProcessArguments(args);
            return command.Execute();
        }
    }
}
