using System.Windows.Input;

namespace Avak.StateMachine.WpfApp.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private IPageViewModel? _pageViewModel;
        public event EventHandler<EventArgs<string>>? ViewChanged;
        private readonly Dictionary<string, IPageViewModel>? _pageViewModels = [];

        public IPageViewModel? CurrentPageViewModel
        {
            get
            {
                return _pageViewModel;
            }
            set
            {
                _pageViewModel = value;
                OnPropertyChanged(nameof(CurrentPageViewModel));
            }
        }

        public MainWindowViewModel()
        {
            _buttonClick = new RelayCommand<string>(ExecuteButtonClick, CanExecuteButtonClick);

            _pageViewModels["1"] = new UserControl1ViewModel("1");

            _pageViewModels["2"] = new UserControl2ViewModel("2");

            _pageViewModels["3"] = new UserControl3ViewModel("3");

            CurrentPageViewModel = _pageViewModels["1"];
        }

        private bool CanExecuteButtonClick(string param)
        {
            return true;
        }

        private void ExecuteButtonClick(string parameter)
        {
            CurrentPageViewModel = _pageViewModels?[parameter];
        }


        private ICommand _buttonClick;

        public ICommand ButtonClick
        {
            get { return _buttonClick; }
        }
    }
}
