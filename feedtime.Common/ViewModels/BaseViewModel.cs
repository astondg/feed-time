namespace FeedTime.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T localField, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(localField, value)) return false;
            localField = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (null != handler)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}