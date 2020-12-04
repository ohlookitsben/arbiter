﻿using Arbiter.Core.Analysis;
using Arbiter.Core.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arbiter.Core.Commands
{
    public class BuildOutputCommand : ICommand
    {
        private readonly RunSettings _settings;
        private readonly IRepositoryReader _repositoryReader;
        private readonly MSBuildSolutionAnalyzer _analyzer;
        private readonly NUnitProjectWriter _writer;

        public BuildOutputCommand(RunSettings settings, IRepositoryReader repositoryReader, MSBuildSolutionAnalyzer analyzer, NUnitProjectWriter writer)
        {
            _settings = settings;
            _repositoryReader = repositoryReader;
            _repositoryReader.WorkingDirectory = _settings.WorkingDirectory;
            _analyzer = analyzer;
            _writer = writer;
        }

        public int Execute()
        {
            var changedFiles = _repositoryReader.ListChangedFiles(_settings.FromCommit, _settings.ToCommit);
            var changedProjects = _analyzer.FindContainingProjects(changedFiles);
            var dependantProjects = _analyzer.FindDependantProjects(changedProjects);
            var dependantProjectPaths = dependantProjects.Select(p => p.Project).ToList();
            _writer.WriteProject(_settings.Output, dependantProjectPaths);
            return 0;
        }
    }
}
