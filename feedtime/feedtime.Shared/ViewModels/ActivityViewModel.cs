namespace FeedTime.ViewModels
{
    using System;
    using System.Windows.Input;
    using FeedTime.Common;
    using FeedTime.Common.DataModel;

    public abstract class ActivityViewModel : BaseViewModel
    {
        private readonly string name;
        private string id;
        private DateTimeOffset startDate;
        private DateTimeOffset? endTime;
        private string notes;
        private Feeling? howBabyFelt;
        private Feeling? howParentFelt;
        private bool nfcInitiated;
        private TimeSpan duration;
        private BabyViewModel baby;
        private bool userHasModifiedDuration;
        private ICommand setHowBabyFelt;
        private ICommand setHowParentFelt;

        protected ActivityViewModel(string name)
        {
            this.name = name;
            UserHasModifiedDuration = false;
        }

        protected bool ServerActionIsRunning { get; set; }

        /// <summary>
        /// The name of the activity, e.g. Sleeping, Feeding
        /// </summary>
        public string Name { get { return name; } }
        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }
        public DateTimeOffset StartDate
        {
            get { return startDate; }
            set
            {
                if (SetProperty(ref startDate, value))
                {
                    NotifyPropertyChanged("StartTime");
                    SetDurationIfNotUserModified();
                }
            }
        }
        public TimeSpan StartTime
        {
            get { return startDate.TimeOfDay; }
            set
            {
                StartDate = startDate.Date.Add(value);
                SetDurationIfNotUserModified();
            }
        }
        public DateTimeOffset? EndTime
        {
            get { return endTime; }
            set { SetProperty(ref endTime, value); }
        }
        public string Notes
        {
            get { return notes; }
            set { SetProperty(ref notes, value); }
        }
        public Feeling? HowBabyFelt
        {
            get { return howBabyFelt; }
            set { SetProperty(ref howBabyFelt, value); }
        }
        public Feeling? HowParentFelt
        {
            get { return howParentFelt; }
            set { SetProperty(ref howParentFelt, value); }
        }
        public bool NfcInitiated
        {
            get { return nfcInitiated; }
            set { SetProperty(ref nfcInitiated, value); }
        }
        public TimeSpan Duration
        {
            get { return duration; }
            set { SetProperty(ref duration, value); }
        }
        public BabyViewModel Baby
        {
            get { return baby; }
            set { SetProperty(ref baby, value); }
        }
        public bool UserHasModifiedDuration
        {
            get { return userHasModifiedDuration; }
            set { SetProperty(ref userHasModifiedDuration, value); }
        }

        public ICommand SetHowBabyFelt
        {
            get
            {
                if (setHowBabyFelt == null)
                {
                    setHowBabyFelt = new RelayCommand<int>(param =>
                    {
                        HowBabyFelt = (Feeling)param;
                    });
                }

                return setHowBabyFelt;
            }
        }

        public ICommand SetHowParentFelt
        {
            get
            {
                if (setHowParentFelt == null)
                {
                    setHowParentFelt = new RelayCommand<int>(param =>
                    {
                        HowParentFelt = (Feeling)param;
                    });
                }

                return setHowParentFelt;
            }
        }

        private void SetDurationIfNotUserModified()
        {
            if (!userHasModifiedDuration)
                Duration = (endTime ?? DateTimeOffset.Now) - startDate;
        }
    }
}