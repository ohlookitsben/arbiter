﻿using Arbiter.MSBuild;
using Arbiter.Tests.Helpers;
using Autofac;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Arbiter.Tests.Integration
{
    [TestFixture]
    [Category(TestCategory.Small)]
    public class MSBuildSolutionAnalyzerTests
    {
        private MSBuildSolutionAnalyzer _analyzer;

        [SetUp]
        public void SetUp()
        {
            var container = ContainerHelper.ConfigureContainer().Build();
            _analyzer = container.Resolve<MSBuildSolutionAnalyzer>();
        }

        [Test]
        public void FindContainingProjects_SolutionNotLoaded_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _analyzer.FindContainingProjects(new List<string> { "Program.cs" }));
        }

        [Test]
        public void FindContainingProjects_FilesIsNull_Throws()
        {
            var analyzer = new MSBuildSolutionAnalyzer(new Mock<IMSBuildSolutionLoader>().Object);

            Assert.Throws<ArgumentNullException>(() => _analyzer.FindContainingProjects(null));
        }
    }
}
