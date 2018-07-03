namespace FeedTime.Converters
{
    using System;
    using Windows.UI.Xaml.Data;

    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return !(bool)value;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return !(bool)value;
            return value;
        }
    }
}