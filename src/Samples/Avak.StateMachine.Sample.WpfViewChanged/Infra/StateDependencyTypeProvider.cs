using Avak.StateMachine.Sample.WpfViewChanged.StateManager.States;
using Avak.StateMachine.Sample.WpfViewChanged.ViewModels;

namespace Avak.StateMachine.Sample.WpfViewChanged.Infra
{
    public class StateDependencyResolverProvider
    {
        private IServiceProvider serviceProvider;
        public StateDependencyResolverProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object? StateDependencyResolverImplimentation(Type stateType)
        {
            object? stateDependency = this.serviceProvider.GetService(stateType);
            return stateDependency;
        }
    }

    public class StateDependencyTypeProvider
    {
        public List<Type?>? StateDependencyTypeFinderImplimentation(Type stateType)
        {
            List<Type?>? dependencyTypes = [];
            Type viewModelType = null!;

            if (stateType == typeof(Aa))
            {
                viewModelType = typeof(UserControl1ViewModel);
            }

            if (stateType == typeof(Bb))
            {
                viewModelType = typeof(UserControl2ViewModel);
            }

            if (stateType == typeof(Cc))
            {
                viewModelType = typeof(UserControl3ViewModel);
            }

            if (stateType == typeof(Dd))
            {
                viewModelType = typeof(UserControl4ViewModel);
            }

            if (viewModelType != null)
            {
                // the following does not seem to work.
                // dependencies.Append(viewModel).ToArray();
                // The following is working.
                dependencyTypes.Add(viewModelType);
            }

            return dependencyTypes;
        }
    }
}
