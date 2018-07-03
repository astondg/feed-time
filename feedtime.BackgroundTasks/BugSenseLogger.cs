namespace FeedTime.BackgroundTasks
{
    using System;
    using System.Threading.Tasks;
    using FeedTime.Common;
    using Windows.Foundation;

    internal class BugSenseLogger : ILogger
    {
        public IAsyncOperation<bool> LogExceptionAsync(Exception ex)
        {
            return LogExceptionInternalAsync(ex).AsAsyncOperation();
        }

        public IAsyncOperation<bool> LogExceptionAsync(Exception ex, string key, string value)
        {
            return LogExceptionInternalAsync(ex, Tuple.Create(key, value)).AsAsyncOperation();
        }

        private Task<bool> LogExceptionInternalAsync(Exception ex, Tuple<string, string> extraData = null)
        {
            return Task.FromResult(true);
        }
    }
}