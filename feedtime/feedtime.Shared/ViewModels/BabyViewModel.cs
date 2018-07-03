namespace FeedTime.ViewModels
{
    using System;
    using System.Windows.Input;
    using FeedTime.Common.Factories;
    using FeedTime.Common;
    using FeedTime.Common.DataModel;
    using FeedTime.Extensions;
    using Windows.Foundation;
    using Windows.UI.Xaml.Media.Imaging;
    
    public class BabyViewModel : BaseViewModel
    {
        private bool serverActionIsRunning;
        private bool showAgeInWeeks;
        private string id;
        private string givenName;
        private string additionalName;
        private string familyName;
        private string profileImagePath;
        private DateTimeOffset dateOfBirth;
        private TimeSpan timeOfBirth;
        private Gender gender;
        private BitmapImage profileImage;
        private FamilyViewModel family;
        private RelayCommand<BabyViewModel> createOrUpdate;
        private ICommand setGender;

        public event TypedEventHandler<BabyViewModel, string> BabyCreated;
        public event TypedEventHandler<BabyViewModel, string> BabyUpdated;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }
        public string GivenName
        {
            get { return givenName; }
            set
            {
                if (SetProperty(ref givenName, value))
                {
                    NotifyPropertyChanged("GivenNames");
                    NotifyPropertyChanged("DisplayName");
                }
            }
        }
        public string GivenNames
        {
            get
            {
                return string.IsNullOrWhiteSpace(additionalName)
                          ? givenName
                          : string.Format("{0} {1}", givenName, additionalName);
            }
        }
        public string AdditionalName
        {
            get { return additionalName; }
            set
            {
                if (SetProperty(ref additionalName, value))
                {
                    NotifyPropertyChanged("GivenNames");
                    NotifyPropertyChanged("DisplayName");
                }
            }
        }
        public string FamilyName
        {
            get { return familyName; }
            set
            {
                if (SetProperty(ref familyName, value))
                {
                    NotifyPropertyChanged("GivenNames");
                    NotifyPropertyChanged("DisplayName");
                }
            }
        }
        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(additionalName)
                            ? string.Format("{0} {1}", givenName, familyName)
                            : string.Format("{0} {1} {2}", givenName, additionalName, familyName); }
        }
        public string ProfileImagePath
        {
            get { return profileImagePath; }
            set
            {
                if (SetProperty(ref profileImagePath, value))
                    LoadProfileImage();
            }
        }
        public BitmapImage ProfileImage
        {
            get { return profileImage; }
            set { SetProperty(ref profileImage, value); }
        }
        public DateTimeOffset DateOfBirth
        {
            get { return dateOfBirth; }
            set
            {
                if (SetProperty(ref dateOfBirth, value))
                    NotifyPropertyChanged("Age");
            }
        }
        public string Age
        {
            get
            {
                var daysSinceBirth = Convert.ToInt64(Math.Floor((DateTimeOffset.Now - dateOfBirth).TotalDays));
                var monthsSinceBirth = MonthDifference(DateTimeOffset.Now, dateOfBirth);
                return showAgeInWeeks
                       ? string.Format("{0:0}w {1:0}d", daysSinceBirth / 7, daysSinceBirth % 7)
                       : string.Format("{0:0} months", monthsSinceBirth);
            }
        }
        public bool ShowAgeInWeeks
        {
            get { return showAgeInWeeks; }
            set
            {
                if (SetProperty(ref showAgeInWeeks, value))
                    NotifyPropertyChanged("Age");
            }
        }
        public TimeSpan TimeOfBirth
        {
            get { return timeOfBirth; }
            set { SetProperty(ref timeOfBirth, value); }
        }
        public Gender Gender
        {
            get { return gender; }
            set { SetProperty(ref gender, value); }
        }
        public FamilyViewModel Family
        {
            get { return family; }
            set { SetProperty(ref family, value); }
        }
        public ICommand CreateOrUpdate
        {
            get
            {
                if (createOrUpdate == null)
                {
                    createOrUpdate = new RelayCommand<BabyViewModel>(async baby =>
                    {
                        try
                        {
                            serverActionIsRunning = true;
                            createOrUpdate.RaiseCanExecuteChanged();
                            if (string.IsNullOrWhiteSpace(baby.Id))
                            {
                                var createdBaby = await DataSourceFactory.Current
                                                                         .CreateBaby(baby.AsModel());
                                baby.Id = createdBaby.Id;
                                if (BabyCreated != null)
                                    BabyCreated(this, baby.id);
                            }
                            else
                            {
                                await DataSourceFactory.Current
                                                       .UpdateBaby(baby.AsModel());
                                if (BabyUpdated != null)
                                    BabyUpdated(this, baby.Id);
                            }
                        }
                        finally
                        {
                            serverActionIsRunning = false;
                            createOrUpdate.RaiseCanExecuteChanged();
                        }
                    }, baby => !serverActionIsRunning && baby != null);
                }

                return createOrUpdate;
            }
        }
        public ICommand SetGender
        {
            get
            {
                if (setGender == null)
                {
                    setGender = new RelayCommand<int>(gender =>
                    {
                        Gender = (Gender)gender;
                    });
                }

                return setGender;
            }
        }

        private static int MonthDifference(DateTimeOffset lValue, DateTimeOffset rValue)
        {
            int monthsDifference = (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);

            // If we haven't reached the same day in lValue month yet then don't count the current month
            return lValue.Day >= rValue.Day ? monthsDifference : monthsDifference - 1;
        }

        private void LoadProfileImage()
        {
            // TODO
        }
    }
}