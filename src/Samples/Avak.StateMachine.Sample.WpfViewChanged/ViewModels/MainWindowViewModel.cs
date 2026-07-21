using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;
using Avak.StateMachine.Sample.WpfViewChanged.Infra;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Reflection;

namespace Avak.StateMachine.Sample.WpfViewChanged.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private IPageViewModel? _currentPageViewModel;

        private readonly Dictionary<string, IPageViewModel>? _pageViewModels = new();

        private readonly IStateMachineManager stateMachineManager = null!;

        private IStateGraph stateGraph = null!;

        public MainWindowViewModel(StateDependencyProvider stateDependencyProvider,
            IStateMachineManager stateMachineManager)
        {
            this.stateMachineManager = stateMachineManager;

            InitializeState();

            var stateList = stateGraph.StateList;

            foreach (var state in stateList)
            {
                IStateViewModel viewModel = state.GetStateViewModel();

                IPageViewModel pageViewModel = (viewModel as IPageViewModel)!;

                pageViewModel.ViewChanged += PageViewModel_ViewChanged;

            }

            IStateViewModel vm = stateMachineManager.CurrentState.GetStateViewModel();

            CurrentPageViewModel = vm as IPageViewModel;
        }

        private void PageViewModel_ViewChanged(object? sender, EventArgs<IPageViewModel> e)
        {
            CurrentPageViewModel = e.Value;
        }

        private void InitializeState()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Sample.WpfViewChanged.StateManager.BasicTransitions.xml";
            Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;

            stateMachineManager.SetMasterStateFile(resourceStream);
            bool loadResult = stateMachineManager.LoadMasterStateFile();
            stateGraph = stateMachineManager.GetStateGraph();

            List<MasterStateBase> states = stateGraph.StateList;

            resourceStream.Close();
            resourceStream.Dispose();
        }
    }
}
