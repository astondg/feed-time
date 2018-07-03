namespace FeedTime.Converters
{
    using FeedTime.ViewModels;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class ActivityViewModelDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FeedTemplate { get; set; }
        public DataTemplate SleepTemplate { get; set; }
        public DataTemplate ChangeTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return SelectTemplateCore(item, null);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is FeedActivityViewModel)
                return FeedTemplate;
            if (item is SleepActivityViewModel)
                return SleepTemplate;
            return ChangeTemplate;
        }
    }
}