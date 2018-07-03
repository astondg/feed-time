namespace FeedTime.Common
{
    using System;
    using Windows.Foundation;

    public interface ILogger
    {
        IAsyncOperation<bool> LogExceptionAsync(Exception ex);
        IAsyncOperation<bool> LogExceptionAsync(Exception ex, string key, string item);
    }
}