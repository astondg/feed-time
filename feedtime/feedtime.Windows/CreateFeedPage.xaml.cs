// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace FeedTime
{
    using System;
    using FeedTime.Common;
    using FeedTime.Pages;
    using FeedTime.Strings;

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CreateFeedPage : CreateFeedPageBase
    {
        public CreateFeedPage()
            : base()
        {
            this.InitializeComponent();

            NavigationHelper.LoadState += NavigationHelper_LoadState;
            NavigationHelper.SaveState += NavigationHelper_SaveState;

            DefaultViewModel.ActivityUpdated += DefaultViewModel_ActivityUpdated;
            DurationPicker.TimeChanged += DurationPicker_TimeChanged;
        }

        private void DefaultViewModel_ActivityUpdated(ViewModels.FeedActivityViewModel sender, string args)
        {
            if (!Frame.Navigate(typeof(MainPage), Constants.NAVIGATION_PARAMETER_CLEARLASTBACKENTRY))
                throw new Exception("Failed to create navigate home after creating feed");
        }
    }
}