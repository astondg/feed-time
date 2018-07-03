namespace FeedTime.Common
{
    using System;
    using System.Threading.Tasks;
    using BugSense;
    using BugSense.Core.Model;
    using Windows.Foundation;

    public class BugSenseLogger : ILogger
    {
        public IAsyncOperation<bool> LogExceptionAsync(Exception ex)
        {
            return LogExceptionInternalAsync(ex).AsAsyncOperation();
        }

        public IAsyncOperation<bool> LogExceptionAsync(Exception ex, string key, string value)
        {
            return LogExceptionInternalAsync(ex, Tuple.Create(key, value)).AsAsyncOperation();
        }

        private async Task<bool> LogExceptionInternalAsync(Exception ex, Tuple<string, string> extraData = null)
        {
            if (extraData == null)
                return (await BugSenseHandler.Instance.LogExceptionAsync(ex)).ResultState == BugSenseResultState.OK;

            var extraDataList = new LimitedCrashExtraDataList();
            extraDataList.Add(extraData.Item1, extraData.Item2);
            return (await BugSenseHandler.Instance.LogExceptionAsync(ex, extraDataList)).ResultState == BugSenseResultState.OK;
        }
    }
}