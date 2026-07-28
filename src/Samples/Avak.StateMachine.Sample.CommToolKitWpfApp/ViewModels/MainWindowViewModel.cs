using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;

namespace Avak.StateMachine.Sample.CommToolKitWpfApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private IPageViewModel? _currentPageViewModel;

        [ObservableProperty]
        private string _message;

        private readonly Dictionary<string, IPageViewModel>? _pageViewModels = [];

        private StateMachineManager stateMachineManager = null!;

        private IStateGraph stateGraph = null!;

        public MainWindowViewModel()
        {
            _message = string.Empty;

            _pageViewModels["Aa"] = new UserControl1ViewModel("Aa");

            _pageViewModels["Bb"] = new UserControl2ViewModel("Bb");

            _pageViewModels["Cc"] = new UserControl3ViewModel("Cc");

            InitializeState();

            CurrentPageViewModel = _pageViewModels![stateMachineManager.CurrentState.Name];
        }

        [RelayCommand()]
        private void OnClick(string arg)
        {
            Message = string.Empty;

            Transition? nextTransition = stateMachineManager.CurrentState.Transitions.FirstOrDefault(transition => transition.Trigger.Name == arg);

            if (nextTransition == null)
            {
                Message = "No transition possible.";
                return;
            }

            Trigger nextStateTrigger = nextTransition.Trigger;

            var result = stateMachineManager.IsTriggeredTriansitionValid(stateMachineManager.CurrentState, nextStateTrigger);

            if (!result.success)
            {
                Message = result.message;
                return;
            }

            stateMachineManager
                .DoTriggeredTriansition(stateMachineManager.CurrentState, nextStateTrigger);

            CurrentPageViewModel = _pageViewModels![stateMachineManager.CurrentState.Name];
        }

        private void InitializeState()
        {
            string masterStateXmlFile = "Avak.StateMachine.Sample.CommToolKitWpfApp.StateManager.BasicTransitions.xml";

            IXmlKeys constants = new XmlKeys();
            stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation, StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            stateMachineManager.LoadMasterStateFile();

            stateGraph = stateMachineManager.GetCurrentStateGraph();

            var t2 = stateMachineManager.CurrentState.Name;
        }
    }
}
