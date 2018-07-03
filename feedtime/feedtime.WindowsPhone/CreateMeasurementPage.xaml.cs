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
    public sealed partial class CreateMeasurementPage : CreateMeasurementPageBase
    {
        public CreateMeasurementPage()
            : base()
        {
            this.InitializeComponent();

            NavigationHelper.LoadState += NavigationHelper_LoadState;
            NavigationHelper.SaveState += NavigationHelper_SaveState;

            DefaultViewModel.MeasurementCreated += DefaultViewModel_MeasurementCreated;
        }

        private void DefaultViewModel_MeasurementCreated(MeasurementViewModel sender, string args)
        {
            Frame.BackStack.RemoveAt(0);
            if (!Frame.Navigate(typeof(MainPage), Constants.NAVIGATION_PARAMETER_CLEARLASTBACKENTRY))
                throw new Exception("Failed to create navigate home after creating measurement");
        }
    }
}