using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FeedTime.Common;
using FeedTime.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace FeedTime
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            // TODO - check if this is needed even with the File Type declaration in the package manifest
            fileOpenPicker.FileTypeFilter.Clear();
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".bmp");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");

            var file = await fileOpenPicker.PickSingleFileAsync();
            DefaultViewModel.ProfileImagePath = file.Path;
        }
    }
}
