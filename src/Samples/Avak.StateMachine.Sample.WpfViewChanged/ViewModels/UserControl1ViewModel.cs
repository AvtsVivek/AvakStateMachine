using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Sample.WpfViewChanged.Infra;
using CommunityToolkit.Mvvm.Input;

namespace Avak.StateMachine.Sample.WpfViewChanged.ViewModels
{
    public partial class UserControl1ViewModel : IPageViewModel
    {
        public string PageId { get; set; }
        public string Title { get; set; }
        public event EventHandler<EventArgs<IPageViewModel>>? ViewChanged;
        private IStateMachineManager stateMachineManager = null!;
        private IStateGraph stateGraph = null!;
        public UserControl1ViewModel(IStateMachineManager stateMachineManager, string pageIndex = "Aa")
        {
            if (stateMachineManager == null)
            {
                throw new ArgumentNullException(nameof(stateMachineManager));
            }

            this.stateMachineManager = stateMachineManager;

            // stateGraph = this.stateMachineManager.GetCurrentStateGraph();

            PageId = pageIndex;
            Title = "View Aa";
        }

        [RelayCommand()]
        private void OnClick(string arg)
        {
            IStateGraph stateGraph = stateMachineManager.GetCurrentStateGraph();
            Trigger nextStateTrigger = stateGraph.TriggerList.First(t => t.Name == arg);

            var result = stateMachineManager.IsTriggeredTriansitionValid(stateMachineManager.CurrentState, nextStateTrigger);

            if (!result.success)
            {
                // Message = result.message;
                return;
            }

            stateMachineManager
                .DoTriggeredTriansition(stateMachineManager.CurrentState, nextStateTrigger);

            IPageViewModel nextViewModel = (stateMachineManager.CurrentState.GetStateViewModel() as IPageViewModel)!;

            ViewChanged?.Invoke(this, new EventArgs<IPageViewModel>(nextViewModel));
        }
    }
}
