// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace FeedTime
{
    using System;
    using FeedTime.Common;
    using FeedTime.Pages;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateBabyPage : CreateBabyPageBase
    {
        public CreateBabyPage()
            : base()
        {
            this.InitializeComponent();

            NavigationHelper.LoadState += this.NavigationHelper_LoadState;
            NavigationHelper.SaveState += this.NavigationHelper_SaveState;

            DefaultViewModel.BabyCreated += DefaultViewModel_BabyCreated;
            DefaultViewModel.BabyUpdated += DefaultViewModel_BabyUpdated;
        }

        protected override void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.NavigationHelper_LoadState(sender, e);

            if (IsExistingBaby)
            {
                PageName.Text = ResourceLoader.GetString("AddBaby_PageTitle_Update");
                CreateOrUpdateButton.Content = ResourceLoader.GetString("CreateOrUpdateButton_Text_Update");
            }
            else
            {
                PageName.Text = ResourceLoader.GetString("AddBaby_PageTitle_Create");
                CreateOrUpdateButton.Content = ResourceLoader.GetString("CreateOrUpdateButton_Text_Create");
            }
        }

        private void DefaultViewModel_BabyCreated(ViewModels.BabyViewModel sender, string args)
        {
            if (!Frame.Navigate(typeof(MainPage), null))
                throw new Exception("Failed to create navigate home after creating feed");
        }

        private void DefaultViewModel_BabyUpdated(ViewModels.BabyViewModel sender, string args)
        {
            if (!Frame.Navigate(typeof(MainPage), null))
                throw new Exception("Failed to create navigate home after creating feed");
        }
    }
}