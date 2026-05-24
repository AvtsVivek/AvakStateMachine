using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
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

		private StateGraph stateGraph = null!;

		public MainWindowViewModel(IStateMachineManager stateMachineManager)
		{
			this.stateMachineManager = stateMachineManager;

			InitializeState();

			_pageViewModels["Aa"] = new UserControl1ViewModel(stateMachineManager, "Aa");
			_pageViewModels["Aa"].ViewChanged += (o, s) =>
			{
				CurrentPageViewModel = _pageViewModels[s.Value];
			};

			_pageViewModels["Bb"] = new UserControl2ViewModel(stateMachineManager, "Bb");
			_pageViewModels["Bb"].ViewChanged += (o, s) =>
			{
				CurrentPageViewModel = _pageViewModels[s.Value];
			};

			//_pageViewModels["Bb"] = new UserControl2ViewModel("Bb");

			// _pageViewModels["Cc"] = new UserControl3ViewModel("Cc");

			CurrentPageViewModel = _pageViewModels![stateMachineManager.CurrentState.Name];
		}

		private void InitializeState()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.Sample.WpfViewChanged.StateManager.BasicTransitions.xml";
			Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;

			IXmlKeys constants = new XmlKeys();

			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();
			stateGraph = stateMachineManager.GetStateGraph();

			List<StateBase> states = stateGraph.StateList;

			// CurrentPageViewModel = _pageViewModels![stateMachineManager.CurrentState.Name];

			resourceStream.Close();
			resourceStream.Dispose();
		}
	}
}
