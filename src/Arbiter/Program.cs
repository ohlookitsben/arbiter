using Arbiter.Core;
using Arbiter.MSBuild;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Arbiter
{
    public class Program
    {
        private readonly ArbiterRootCommand _rootCommand;
        private readonly IConsole _console;
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
                Console.Error.WriteLine($"A fatal exception occurred before processing could start:{Environment.NewLine}{e}");

                return -1;
            }

            return program.Run(args);
        }

        /// <summary>
        /// <see cref="LoadProgram"/> references code that requires MSBuild assemblies to be loaded so it must be in a separate method to the <see cref="ArbiterMSBuildLocator.RegisterDefaults"/> call.
        /// </summary>
        private static Program LoadProgram()
        {
            var builder = new ContainerBuilder();
            // Register Arbiter types as themselves and their interfaces.
            var assemblies = new[] { typeof(Program).Assembly, typeof(ArbiterRootCommand).Assembly, typeof(MSBuildSolutionAnalyzer).Assembly };
            builder.RegisterAssemblyTypes(assemblies)
                .PublicOnly().Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any())
                .AsSelf().AsImplementedInterfaces();

            builder.RegisterLogger();

            // Setup a real console for System.CommandLine to use.
            builder.RegisterType<SystemConsole>().As<IConsole>();

            // Setup a real FileSystem for System.IO.Abstractions to use.
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Store the working directory and ensure PowerShell calls run in it.
            string workingDirectory = Directory.GetCurrentDirectory();
            builder.RegisterType<PowerShellInvoker>().As<IPowerShellInvoker>().WithParameter(new TypedParameter(typeof(string), workingDirectory));

            var container = builder.Build();
            return container.Resolve<Program>();
        }

        public Program(ArbiterRootCommand rootCommand, IConsole console, ILogger log)
        {
            _rootCommand = rootCommand;
            _console = console;
            _log = log;
        }

        public int Run(string[] args)
        {
            try
            {
                return _rootCommand.InvokeAsync(args, _console).Result;
            }
            catch (Exception e)
            {
                _console.Error.WriteLine($"A {e.GetType().Name} occurred during processing. See the log file {Constants.LogFile} for details.");
                _log.Error(e.ToString());

                return -1;
            }
        }
    }
}
