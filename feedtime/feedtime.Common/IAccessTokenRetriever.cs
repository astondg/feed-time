namespace FeedTime.Common
{
    using System;
    using Microsoft.WindowsAzure.MobileServices;
    using Windows.Foundation;

    public interface IAccessTokenRetriever
    {
        string UserId { get; }
        string AccessToken { get; }
        IAsyncAction RefreshUserIdAndAccessToken(object mobileService);
        bool CompleteLogin(object mobileService);
        event TypedEventHandler<IAccessTokenRetriever, bool> LoginCompleted;
    }
}