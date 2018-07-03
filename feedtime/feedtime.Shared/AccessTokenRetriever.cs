namespace FeedTime
{
    using System;
    using System.Threading.Tasks;
    using FeedTime.Common;
    using Microsoft.WindowsAzure.MobileServices;
    using Windows.ApplicationModel.Activation;
    using Windows.Foundation;
    using Windows.Security.Authentication.Web;

    internal class AccessTokenRetriever : IAccessTokenRetriever
    {
        public string UserId { get; private set; }

        public string AccessToken { get; private set; }

        public event TypedEventHandler<IAccessTokenRetriever, bool> LoginCompleted;

#if WINDOWS_PHONE_APP
        public WebAuthenticationBrokerContinuationEventArgs LoginContinuationArguments { get; set; }
#endif

        public IAsyncAction RefreshUserIdAndAccessToken(object mobileService)
        {
            return RefreshUserIdAndAccessTokenInternal((MobileServiceClient)mobileService).AsAsyncAction();
        }

        /// <summary>
        /// Check if the OAuth process was successful, 
        /// retrieve the OAuth response code and swap it for a pair of access & refresh tokens
        /// </summary>
        /// <returns>True if the data source was authorised and an access token was retrieved</returns>
        public bool CompleteLogin(object mobileService)
        {
#if WINDOWS_PHONE_APP
            var castMobileService = (MobileServiceClient)mobileService;
            castMobileService.LoginComplete(LoginContinuationArguments);
            if (castMobileService.CurrentUser != null)
            {
                UserId = castMobileService.CurrentUser.UserId;
                AccessToken = castMobileService.CurrentUser.MobileServiceAuthenticationToken;
            }

            bool loginWasSuccessful = LoginContinuationArguments.WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success;

            if (LoginCompleted != null)
                LoginCompleted(this, loginWasSuccessful);

            return loginWasSuccessful;
#else
            return true;
#endif
        }

        private async Task RefreshUserIdAndAccessTokenInternal(MobileServiceClient mobileService)
        {
            try
            {
                var user = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                UserId = user.UserId;
                AccessToken = user.MobileServiceAuthenticationToken;
            }
            catch (Exception ex)
            {
                UserId = null;
                AccessToken = null;
            }
        }
    }
}