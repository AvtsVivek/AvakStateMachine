using Avak.StateMachine.Sample.WpfViewChanged.StateManager.States;
using Avak.StateMachine.Sample.WpfViewChanged.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Avak.StateMachine.Sample.WpfViewChanged.Infra
{
	public class StateDependencyProvider
	{
		private IServiceProvider serviceProvider;
		public StateDependencyProvider(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public List<object?>? StateDependencyTypeFinderImplimentation(Type stateType)
		{
			List<object?>? dependencies = [];
			object viewModel = null!;

			if (stateType == typeof(Aa))
			{
				viewModel = this.serviceProvider.GetRequiredService<UserControl1ViewModel>();
			}

			if (stateType == typeof(Bb))
			{
				viewModel = this.serviceProvider.GetRequiredService<UserControl2ViewModel>();
			}

			if (stateType == typeof(Cc))
			{
				viewModel = this.serviceProvider.GetRequiredService<UserControl3ViewModel>();
			}

			if (viewModel != null)
			{
				// the following does not seem to work.
				// dependencies.Append(viewModel).ToArray();
				// The following is working.
				dependencies.Add(viewModel);
			}

			return dependencies;
		}
	}
}
