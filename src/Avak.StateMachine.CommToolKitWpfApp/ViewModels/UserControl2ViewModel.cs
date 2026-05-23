namespace Avak.StateMachine.CommToolKitWpfApp.ViewModels
{
	public class UserControl2ViewModel : IPageViewModel
	{
		public string PageId { get; set; }
		public string Title { get; set; }

		public UserControl2ViewModel(string pageIndex = "Bb")
		{
			PageId = pageIndex;
			Title = "View Bb";
		}
	}
}
