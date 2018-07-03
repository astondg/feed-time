namespace FeedTime.Pages
{
    using System;
    using System.Threading;
    using FeedTime.Common;
    using FeedTime.Extensions;
    using FeedTime.ViewModels;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public class CreateMeasurementPageBase : Page
    {
        private NavigationHelper navigationHelper;
        private MeasurementViewModel viewModel;

        public CreateMeasurementPageBase()
        {
            viewModel = new MeasurementViewModel();
            this.navigationHelper = new NavigationHelper(this);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        
        /// <summary>
        /// Gets the view model for this <see cref="Page"/>. This can be changed to a strongly typed view model.
        /// </summary>
        public MeasurementViewModel DefaultViewModel
        {
            get { return viewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        protected void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var navigationParameter = e.NavigationParameter as ViewModelNavigationParameter<MeasurementViewModel>;

            if (navigationParameter != null)
            {
                // If we have been passed an existing change then display it
                if (navigationParameter.ViewModel != null)
                    viewModel.From(navigationParameter.ViewModel);
            }

            if (viewModel.Baby == null)
                viewModel.Baby = new BabyViewModel { Id = SettingsViewModel.Current.MostRecentlyAccessedBabyId };
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        protected void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        { }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}