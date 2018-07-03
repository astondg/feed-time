namespace FeedTime
{
    using System;
    using FeedTime.Common;
    using FeedTime.Pages;
    using FeedTime.Strings;
    using FeedTime.ViewModels;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateChangePage : CreateChangePageBase
    {
        public CreateChangePage()
            : base()
        {
            this.InitializeComponent();

            NavigationHelper.LoadState += NavigationHelper_LoadState;
            NavigationHelper.SaveState += NavigationHelper_SaveState;

            DefaultViewModel.ActivityUpdated += DefaultViewModel_ActivityUpdated;
        }

        private void DefaultViewModel_ActivityUpdated(ChangeActivityViewModel sender, string args)
        {
            if (!Frame.Navigate(typeof(MainPage), Constants.NAVIGATION_PARAMETER_CLEARLASTBACKENTRY))
                throw new Exception("Failed to create navigate home after creating change");
        }
    }
}