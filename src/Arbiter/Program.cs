using Arbiter.Core;
using Arbiter.Core.Analysis;
using Arbiter.MSBuild;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using System;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Console = System.Console;

namespace Arbiter
{
    public class Program
    {
        private readonly ArbiterRootCommand _rootCommand;
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
                var locator = new ArbiterMSBuildLocator();
                locator.RegisterDefaults();

                program = LoadProgram(locator);
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
        /// <see cref="LoadProgram"/> references code that requires MSBuild assemblies to be loaded so it must be in a separate method to the <see cref="ArbiterMSBuildLocator.RegisterDefaults"/> call.
        /// </summary>
        private static Program LoadProgram(ArbiterMSBuildLocator msBuildLocator)
        {
            var builder = new ContainerBuilder();
            var assemblies = new[] { typeof(Program).Assembly, typeof(ArbiterRootCommand).Assembly, typeof(MSBuildSolutionAnalyzer).Assembly };
            builder.RegisterAssemblyTypes(assemblies)
                .PublicOnly().Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
                .AsSelf().AsImplementedInterfaces();
            builder.RegisterLogger();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            string workingDirectory = Directory.GetCurrentDirectory();
            builder.RegisterType<PowerShellInvoker>().As<IPowerShellInvoker>().WithParameter(new TypedParameter(typeof(string), workingDirectory));
            builder.RegisterInstance(msBuildLocator).As<IMSBuildLocator>().SingleInstance();

            var container = builder.Build();
            return container.Resolve<Program>();
        }

        public Program(ArbiterRootCommand rootCommand, ILogger log)
        {
            _rootCommand = rootCommand;
            _log = log;
        }

        public int Run(string[] args)
        {
            try
            {
                return _rootCommand.InvokeAsync(args).Result;
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
