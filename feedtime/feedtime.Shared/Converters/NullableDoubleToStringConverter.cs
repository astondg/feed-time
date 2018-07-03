namespace FeedTime.Converters
{
    using System;
    using System.Globalization;
    using Windows.UI.Xaml.Data;

    public class NullableDoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueAsDouble = value as double?;
            return valueAsDouble.HasValue ? valueAsDouble.Value.ToString() : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var valueAsString = value as string;
            if (string.IsNullOrWhiteSpace(valueAsString)) return (double?)null;

            var cultureInfo = new CultureInfo(language);
            double parsedValue;
            return double.TryParse(valueAsString, NumberStyles.Any, cultureInfo, out parsedValue) ? parsedValue : (double?)null;
        }
    }
}