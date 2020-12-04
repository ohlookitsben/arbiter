using System;
using System.Collections.Generic;
using System.Text;

namespace Arbiter.Core.Commands
{
    public class BuildOutputCommand : ICommand
    {
        private readonly RunSettings _settings;

        public BuildOutputCommand(RunSettings settings)
        {
            _settings = settings;
        }

        public int Execute()
        {
            return 0;
        }
    }
}
