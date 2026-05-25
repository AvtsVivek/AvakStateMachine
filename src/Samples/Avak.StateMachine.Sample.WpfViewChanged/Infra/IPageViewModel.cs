using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Sample.WpfViewChanged.Infra;

namespace Avak.StateMachine.Sample.WpfViewChanged
{
	public interface IPageViewModel : IStateViewModel
	{
		event EventHandler<EventArgs<IPageViewModel>>? ViewChanged;
		string PageId { get; set; }
		string Title { get; set; }
	}
}
