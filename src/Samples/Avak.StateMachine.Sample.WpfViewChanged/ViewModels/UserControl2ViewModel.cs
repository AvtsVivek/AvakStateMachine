using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Sample.WpfViewChanged.Infra;
using CommunityToolkit.Mvvm.Input;

namespace Avak.StateMachine.Sample.WpfViewChanged.ViewModels
{
	public partial class UserControl2ViewModel : IPageViewModel
	{
		public string PageId { get; set; }
		public string Title { get; set; }

		public event EventHandler<EventArgs<string>>? ViewChanged;
		private IStateMachineManager stateMachineManager = null!;
		private StateGraph stateGraph = null!;
		public UserControl2ViewModel(IStateMachineManager stateMachineManager, string pageIndex = "Aa")
		{
			if (stateMachineManager == null)
			{
				throw new ArgumentNullException(nameof(stateMachineManager));
			}

			this.stateMachineManager = stateMachineManager;

			stateGraph = this.stateMachineManager.GetStateGraph();

			PageId = pageIndex;
			Title = "View Bb";
		}

		[RelayCommand()]
		private void OnClick(string arg)
		{
			Trigger nextStateTrigger = stateGraph.TriggerList.First(t => t.Name == arg);

			var result = stateMachineManager.IsTriggeredTriansitionValid(stateMachineManager.CurrentState, nextStateTrigger);

			if (!result.success)
			{
				// Message = result.message;
				return;
			}

			stateMachineManager
				.DoTriggeredTriansition(stateMachineManager.CurrentState, nextStateTrigger);

			ViewChanged?.Invoke(this, new EventArgs<string>("Cc"));
		}
	}
}
