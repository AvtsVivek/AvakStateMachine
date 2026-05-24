namespace Avak.StateMachine.WpfApp.ViewModels
{
    public class UserControl2ViewModel : IPageViewModel
    {
        public event EventHandler<EventArgs<string>>? ViewChanged;
        public string PageId { get; set; }
        public string Title { get; set; }

        public UserControl2ViewModel(string pageIndex = "2")
        {
            PageId = pageIndex;
            Title = "View 2";
        }
    }
}
