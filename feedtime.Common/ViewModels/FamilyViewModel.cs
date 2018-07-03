namespace FeedTime.ViewModels
{
    public class FamilyViewModel : BaseViewModel
    {
        private string id;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }
    }
}