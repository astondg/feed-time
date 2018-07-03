namespace FeedTime.Common
{
    using FeedTime.ViewModels;

    public class ViewModelNavigationParameter<TViewModel> where TViewModel : BaseViewModel
    {
        public ViewModelNavigationParameter()
        { }

        public ViewModelNavigationParameter(string flag)
            : this(flag, null) { }

        public ViewModelNavigationParameter(TViewModel viewModel)
            : this(null, viewModel) { }

        public ViewModelNavigationParameter(string flag, TViewModel viewModel)
        {
            Flag = flag;
            ViewModel = viewModel;
        }

        public string Flag { get; set; }
        public TViewModel ViewModel { get; set; }
    }
}