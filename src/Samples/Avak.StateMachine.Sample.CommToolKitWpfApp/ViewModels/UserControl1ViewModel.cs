namespace Avak.StateMachine.Sample.CommToolKitWpfApp.ViewModels
{
	public class UserControl1ViewModel : IPageViewModel
	{
		public string PageId { get; set; }
		public string Title { get; set; }

		public UserControl1ViewModel(string pageIndex = "Aa")
		{
			PageId = pageIndex;
			Title = "View Aa";
		}
	}
}
