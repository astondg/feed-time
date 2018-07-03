namespace FeedTime.Converters
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool assert = parameter == null || !parameter.ToString().Equals("NEGATE", StringComparison.OrdinalIgnoreCase);
            bool valueAsBool = value != null;
            return (valueAsBool && assert) || (!valueAsBool && !assert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}