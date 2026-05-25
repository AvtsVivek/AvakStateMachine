using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
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

		// private StateDependencyProvider stateDependencyProvider;

		private StateGraph stateGraph = null!;

		public MainWindowViewModel(StateDependencyProvider stateDependencyProvider,
			IStateMachineManager stateMachineManager)
		{
			this.stateMachineManager = stateMachineManager;

			// this.stateDependencyProvider = stateDependencyProvider;

			InitializeState();

			var stateList = stateGraph.StateList;

			foreach (var state in stateList)
			{
				IStateViewModel viewModel = state.GetStateViewModel();

				IPageViewModel pageViewModel = (viewModel as IPageViewModel)!;

				pageViewModel.ViewChanged += PageViewModel_ViewChanged;

			}


			//_pageViewModels["Aa"].ViewChanged += (o, s) =>
			//{
			//	CurrentPageViewModel = _pageViewModels[s.Value];
			//};

			//_pageViewModels["Bb"].ViewChanged += (o, s) =>
			//{
			//	CurrentPageViewModel = _pageViewModels[s.Value];
			//};

			//_pageViewModels["Cc"].ViewChanged += (o, s) =>
			//{
			//	CurrentPageViewModel = _pageViewModels[s.Value];
			//};

			//_pageViewModels["Bb"] = new UserControl2ViewModel("Bb");

			// _pageViewModels["Cc"] = new UserControl3ViewModel("Cc");

			// CurrentPageViewModel = _pageViewModels![stateMachineManager.CurrentState.Name];

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

			//IXmlKeys constants = new XmlKeys();

			//StateMachineManager stateMachineManager = new(constants,
			//	this.stateDependencyProvider.StateDependencyTypeFinderImplimentation);

			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();
			stateGraph = stateMachineManager.GetStateGraph();

			List<StateBase> states = stateGraph.StateList;

			// SendNotification(stateMachineManager.CurrentState, StateDependencyTypeFinderImplimentation);
			resourceStream.Close();
			resourceStream.Dispose();
		}
	}
}
