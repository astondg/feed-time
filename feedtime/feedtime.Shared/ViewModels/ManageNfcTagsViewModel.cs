namespace FeedTime.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using FeedTime.Common;

    public class ManageNfcTagsViewModel : BaseViewModel
    {
        private bool creatingTag;
        private BabyViewModel currentBaby;
        private ICommand create;

        public ManageNfcTagsViewModel()
        {
            Babies = new ObservableCollection<BabyViewModel>();
        }

        public BabyViewModel CurrentBaby
        {
            get { return currentBaby; }
            set { SetProperty(ref currentBaby, value); }
        }

        public bool FamilyHasMultipleBabies
        {
            get { return Babies.Count > 0; }
        }

        public ObservableCollection<BabyViewModel> Babies { get; private set; }

        public ICommand Create
        {
            get
            {
                if (create == null)
                {
                    create = new RelayCommand<string>(type =>
                    {
                        creatingTag = true;
                        switch (type)
                        {
                            case "feed":
                                NfcService.WriteFeedTag(currentBaby.Id);
                                break;
                            case "sleep":
                                NfcService.WriteSleepTag(currentBaby.Id);
                                break;
                            case "change":
                                NfcService.WriteChangeTag(currentBaby.Id);
                                break;
                            default:
                                break;
                        }
                        creatingTag = false;
                    }, type => !creatingTag);
                }

                return create;
            }
        }
    }
}