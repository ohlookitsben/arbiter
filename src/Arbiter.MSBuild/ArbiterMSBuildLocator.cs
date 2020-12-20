using Arbiter.Core.Analysis;
using Microsoft.Build.Locator;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Arbiter.MSBuild
{
    public class ArbiterMSBuildLocator : IMSBuildLocator
    {
        // Only one registration works, so being static is not an issue here.
        private static VisualStudioInstance _registeredInstance;

        /// <summary>
        /// When the ComReference target exists in a project, assembly resolution fails. Copying the assemblies
        /// to the application directory allows the resolution to succeed and for projects referencing COM
        /// assemblies to be used.
        /// </summary>
        private readonly string[] _comResolutionAssemblies = new[]
        {
            "Microsoft.Build.dll",
            "Microsoft.Build.Framework.dll",
            "Microsoft.Build.Tasks.Core.dll",
            "Microsoft.Build.Utilities.Core.dll"
        };

        public void RegisterDefaults()
        {
            if (_registeredInstance != null)
            {
                // An instance has been registered. Registering again is an error.
                return;
            }

            _registeredInstance = MSBuildLocator.RegisterDefaults();
        }

        public void SetupCom()
        {
            if (_registeredInstance == null)
            {
                throw new InvalidOperationException("Cannot copy assemblies until the MSBuild instance has been registered.");
            }

            var assemblyPaths = _comResolutionAssemblies.Select(a => Path.Combine(_registeredInstance.MSBuildPath, a));
            foreach (var path in assemblyPaths)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Failed to find required assembly for COM resolution at: {path}");
                }
            }

            string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var path in assemblyPaths)
            {
                string fileName = Path.GetFileName(path);
                string destination = Path.Combine(applicationDirectory, fileName);
                if (File.Exists(destination))
                {
                    continue;
                }

                File.Copy(path, destination);
            }
        }

        public void Clean()
        {
            if (_registeredInstance == null)
            {
                throw new InvalidOperationException("Cannot clean assemblies until the MSBuild instance has been registered.");
            }

            var assemblyPaths = _comResolutionAssemblies.Select(a => Path.Combine(_registeredInstance.MSBuildPath, a));
            foreach (var path in assemblyPaths)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Failed to find required assembly for COM resolution at: {path}");
                }
            }

            string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var path in assemblyPaths)
            {
                string fileName = Path.GetFileName(path);
                string destination = Path.Combine(applicationDirectory, fileName);
                if (File.Exists(destination))
                {
                    try
                    {
                        File.Delete(destination);
                    }
                    catch
                    {
                        // This is only a best-effort clean.
                    }
                }
            }
        }
    }
}
