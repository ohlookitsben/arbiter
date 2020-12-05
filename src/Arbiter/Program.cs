﻿using Arbiter.Core;
using Arbiter.MSBuild;
using Autofac;
using System;

namespace Arbiter
{
    internal class Program
    {
        private readonly CommandBuilder _commandBuilder;

        private static int Main(string[] args)
        {
            Program program;
            try
            {
                var builder = new ContainerBuilder();
                var assemblies = new[] { typeof(Program).Assembly, typeof(CommandBuilder).Assembly, typeof(MSBuildSolutionAnalyzer).Assembly };
                builder.RegisterAssemblyTypes(assemblies).PublicOnly().AsSelf().AsImplementedInterfaces();
                var container = builder.Build();
                program = container.Resolve<Program>();
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal Exception:");
                Console.WriteLine(e);
                return -1;
            }

            return program.Run(args);
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
