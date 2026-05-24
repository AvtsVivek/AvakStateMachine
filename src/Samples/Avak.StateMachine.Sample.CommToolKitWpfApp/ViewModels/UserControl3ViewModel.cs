namespace Avak.StateMachine.Sample.CommToolKitWpfApp.ViewModels
{
	public class UserControl3ViewModel : IPageViewModel
	{
		public string PageId { get; set; }
		public string Title { get; set; }

		public UserControl3ViewModel(string pageIndex = "Cc")
		{
			PageId = pageIndex;
			Title = "View Cc";
		}
	}
}
