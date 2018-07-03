namespace FeedTime.ViewModels
{
    using System;
    using System.Windows.Input;
    using FeedTime.Common;
    using FeedTime.DataModel;
    using FeedTime.Extensions;

    public class BabyViewModel : BaseViewModel
    {
        private bool dataOperationProgress;
        private string id;
        private string givenName;
        private string additionalName;
        private string familyName;
        private DateTimeOffset dateOfBirth;
        private TimeSpan timeOfBirth;
        private Gender gender;
        private FamilyViewModel family;
        private ICommand createBaby;
        private ICommand setGender;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }
        public string GivenName
        {
            get { return givenName; }
            set { SetProperty(ref givenName, value); }
        }
        public string AdditionalName
        {
            get { return additionalName; }
            set { SetProperty(ref additionalName, value); }
        }
        public string FamilyName
        {
            get { return familyName; }
            set { SetProperty(ref familyName, value); }
        }
        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(additionalName)
                            ? string.Format("{0} {1}", givenName, familyName)
                            : string.Format("{0} {1} {2}", givenName, additionalName, familyName); }
        }
        public DateTimeOffset DateOfBirth
        {
            get { return dateOfBirth; }
            set { SetProperty(ref dateOfBirth, value); }
        }
        public TimeSpan TimeOfBirth
        {
            get { return timeOfBirth; }
            set { SetProperty(ref timeOfBirth, value); }
        }
        public string Age
        {
            get { return MonthDifference(DateTime.Now, dateOfBirth) + " months"; }
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
        public ICommand Create
        {
            get
            {
                if (createBaby == null)
                {
                    createBaby = new RelayCommand<BabyViewModel>(async baby =>
                    {
                        dataOperationProgress = true;
                        var createdBaby = await MobileServicesDataSource.Current
                                                                        .CreateBaby(baby.AsModel());
                        baby.Id = createdBaby.Id;
                        dataOperationProgress = false;
                    }, baby => !dataOperationProgress && string.IsNullOrWhiteSpace(baby.Id));
                }

                return createBaby;
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

                return createBaby;
            }
        }

        private static int MonthDifference(DateTimeOffset lValue, DateTimeOffset rValue)
        {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }
    }
}