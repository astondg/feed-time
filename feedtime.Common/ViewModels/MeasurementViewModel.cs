namespace FeedTime.ViewModels
{
    using System;

    public class MeasurementViewModel : BaseViewModel
    {
        private string id;
        private double? length;
        private double? weight;
        private DateTimeOffset? createdAt;
        private BabyViewModel baby;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }
        public double? Length
        {
            get { return length; }
            set { SetProperty(ref length, value); }
        }
        public double? Weight
        {
            get { return weight; }
            set { SetProperty(ref weight, value); }
        }
        public DateTimeOffset? CreatedAt
        {
            get { return createdAt; }
            set { SetProperty(ref createdAt, value); }
        }
        public BabyViewModel Baby
        {
            get { return baby; }
            set { SetProperty(ref baby, value); }
        }
    }
}