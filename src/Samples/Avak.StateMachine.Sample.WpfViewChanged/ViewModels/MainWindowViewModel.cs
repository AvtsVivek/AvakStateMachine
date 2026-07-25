using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;
using Avak.StateMachine.Sample.WpfViewChanged.Infra;
using CommunityToolkit.Mvvm.ComponentModel;
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

        public MainWindowViewModel(IStateMachineManager stateMachineManager)
        {
            this.stateMachineManager = stateMachineManager;

            this.stateMachineManager.StateCreated += StateMachineManager_StateCreated;

            InitializeState();

            IStateViewModel viewModel = stateMachineManager.CurrentState.GetStateViewModel();
            CurrentPageViewModel = viewModel as IPageViewModel;
        }

        private void StateMachineManager_StateCreated(object? sender, StateBase state)
        {
            IStateViewModel viewModel = state.GetStateViewModel();

            IPageViewModel pageViewModel = (viewModel as IPageViewModel)!;

            pageViewModel.ViewChanged += PageViewModel_ViewChanged;
        }

        private void PageViewModel_ViewChanged(object? sender, EventArgs<IPageViewModel> e)
        {
            CurrentPageViewModel = e.Value;
        }

        private void InitializeState()
        {
            string masterStateXmlFile = "Avak.StateMachine.Sample.WpfViewChanged.StateManager.BasicTransitions.xml";

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            stateMachineManager.LoadMasterStateFile();

            stateGraph = stateMachineManager.GetCurrentStateGraph();
            List<MasterStateBase> stateList = stateGraph.StateList;

        }
    }
}
