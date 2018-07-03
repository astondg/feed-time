namespace FeedTime.BackgroundTasks
{
    using System;
    using System.Threading.Tasks;
    using FeedTime.Common;
    using Windows.Foundation;

    internal class AccessTokenRetriever : IAccessTokenRetriever
    {
        public string UserId { get; private set; }

        public string AccessToken { get; private set; }

        public event TypedEventHandler<IAccessTokenRetriever, bool> LoginCompleted;

        public IAsyncAction RefreshUserIdAndAccessToken(object mobileService)
        {
            UserId = null;
            AccessToken = null;
            return Task.FromResult(true).AsAsyncAction();
        }

        public bool CompleteLogin(object mobileService)
        {
            throw new NotImplementedException();
        }
    }
}