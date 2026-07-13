using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
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

		private StateGraph stateGraph = null!;

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
			Trigger nextStateTrigger = stateGraph.TriggerList.First(t => t.Name == arg);

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
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.Sample.CommToolKitWpfApp.StateManager.BasicTransitions.xml";
			Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;

			IXmlKeys constants = new XmlKeys();
			stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);

			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();
			stateGraph = stateMachineManager.GetStateGraph();

			List<StateBase> states = stateGraph.StateList;

			// 

			resourceStream.Close();
			resourceStream.Dispose();
		}
	}
}
