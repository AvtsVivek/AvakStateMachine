using Avak.StateMachine.Sample.WpfViewChanged.Infra;

namespace Avak.StateMachine.Sample.WpfViewChanged
{
	public interface IPageViewModel
	{
		event EventHandler<EventArgs<string>>? ViewChanged;
		string PageId { get; set; }
		string Title { get; set; }
	}
}
