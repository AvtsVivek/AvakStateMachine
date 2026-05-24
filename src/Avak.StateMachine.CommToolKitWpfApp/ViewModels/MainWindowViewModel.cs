using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Reflection;

namespace Avak.StateMachine.CommToolKitWpfApp.ViewModels
{
	public partial class MainWindowViewModel : ObservableObject
	{
		[ObservableProperty]
		private IPageViewModel? _currentPageViewModel;

		private readonly Dictionary<string, IPageViewModel>? _pageViewModels = [];

		private StateMachineManager stateMachineManager = null!;

		private StateBase currentState = null!;

		private StateGraph stateGraph = null!;

		public MainWindowViewModel()
		{
			_pageViewModels["Aa"] = new UserControl1ViewModel("Aa");

			_pageViewModels["Bb"] = new UserControl2ViewModel("Bb");

			_pageViewModels["Cc"] = new UserControl3ViewModel("Cc");

			InitializeState();
		}

		[RelayCommand()]
		private void OnClick(string arg)
		{
			Trigger enterNextStateTrigger = stateGraph.TriggerList.First(t => t.Name == arg);
			(bool, string) result = stateMachineManager
				.DoTriggeredTriansition(stateMachineManager.CurrentState, enterNextStateTrigger);

			CurrentPageViewModel = _pageViewModels![stateMachineManager.CurrentState.Name];
		}

		private void InitializeState()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.CommToolKitWpfApp.StateManager.BasicTransitions.xml";
			Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;

			IXmlKeys constants = new XmlKeys();
			// IStateFileReader reader = new XmlStateFileReader(constants);
			stateMachineManager = new(constants);

			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();
			stateGraph = stateMachineManager.GetStateGraph();

			List<StateBase> states = stateGraph.StateList;

			// currentState = stateMachineManager.CurrentState;

			CurrentPageViewModel = _pageViewModels![stateMachineManager.CurrentState.Name];

			resourceStream.Close();
			resourceStream.Dispose();
		}
	}
}
