using Autofac;

namespace Arbiter.Core.Commands
{
    public class CommandFactory
    {
        private readonly ILifetimeScope _scope;

        public CommandFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public PrintMessageCommand CreatePrintMessageCommand(string message) => _scope.Resolve<PrintMessageCommand>(new TypedParameter(typeof(string), message));

        public BuildOutputCommand CreateBuildOutputCommand(RunSettings settings) => _scope.Resolve<BuildOutputCommand>(new TypedParameter(typeof(RunSettings), settings));

        public ICommand CreateCommand<T>() where T : ICommand => _scope.Resolve<T>();
    }
}
