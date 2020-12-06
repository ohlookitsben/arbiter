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
        private readonly ILogger _log;

        private static int Main(string[] args)
        {
            Program program;
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(Constants.LogFile)
                    .CreateLogger();

                // The correct MSBuild must be registered before types that reference it cause the assemblies to load.
                new ArbiterMSBuildLocator().RegisterDefaults();

                program = LoadProgram();
            }
            catch (Exception e)
            {
                Console.WriteLine("A fatal exception occurred before processing could start:");
                Console.WriteLine(e);

                return -1;
            }

            return program.Run(args);
        }

        /// <summary>
        /// LoadProgram references code that requires MSBuild assemblies to be loaded so it must be in a separate method to the RegisterDefaults call.
        /// </summary>
        private static Program LoadProgram()
        {
            var builder = new ContainerBuilder();
            var assemblies = new[] { typeof(Program).Assembly, typeof(CommandBuilder).Assembly, typeof(MSBuildSolutionAnalyzer).Assembly };
            builder.RegisterAssemblyTypes(assemblies).PublicOnly().AsSelf().AsImplementedInterfaces();
            builder.RegisterLogger();

            var container = builder.Build();
            return container.Resolve<Program>();
        }

        public Program(CommandBuilder commandBuilder, ILogger log)
        {
            _commandBuilder = commandBuilder;
            _log = log;
        }

        public int Run(string[] args)
        {
            try
            {
                var command = _commandBuilder.ProcessArguments(args);
                return command.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine($"A {e.GetType().Name} occurred during processing. See the log file {Constants.LogFile} for details.");
                _log.Error(e.ToString());

                return -1;
            }
        }
    }
}
