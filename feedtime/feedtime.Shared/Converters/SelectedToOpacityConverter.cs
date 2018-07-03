namespace FeedTime.Converters
{
    using System;
    using Windows.UI.Xaml.Data;

    public class SelectedToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return 0.25;
            if (parameter == null) return 0.25;
            return System.Convert.ChangeType(value, parameter.GetType()).Equals(parameter) ? 1 : 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}