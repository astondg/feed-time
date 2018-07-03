namespace FeedTime.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FeedTime.Extensions;
    using FeedTime.Strings;
    using FeedTime.ViewModels;
    using Microsoft.WindowsAzure.MobileServices;
    using Windows.ApplicationModel.Activation;
    using Windows.Security.Authentication.Web;
    using Windows.Security.Credentials;
    using Windows.Storage;

    public class MobileServicesDataSource
    {
        private const string CREDENTIAL_ACCESSTOKEN_MOBILESERVICES = "MobileServicesAcessToken";

        //#if DEBUG && WINDOWS_PHONE_APP
        //    private static MobileServiceClient mobileService = new MobileServiceClient(
        //        "https://192.168.1.112:51538"
        //    );
        //#elif DEBUG
        //private static MobileServiceClient mobileService = new MobileServiceClient(
        //    "http://localhost:51538"
        //);
        //#else
            private static MobileServiceClient mobileService = new MobileServiceClient(
                 Constants.URL_MOBILESERVICES,
                 Constants.OAUTH_APPKEY_MOBILESERVICES);
        //#endif

        private static MobileServicesDataSource current;

        static MobileServicesDataSource()
        {
            current = new MobileServicesDataSource(SettingsViewModel.Current.MobileServicesUserId);
        }

        public static MobileServicesDataSource Current { get { return current; } }

        public MobileServicesDataSource(string userId)
        {
            User = mobileService.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = AccessToken
            };
        }

        public MobileServiceUser User { get; private set; }

        /// <summary>
        /// The Mobile Services access token which is stored as a credential in the PasswordVault
        /// </summary>
        private string AccessToken
        {
            get
            {
                var passwordVault = new PasswordVault();
                try
                {
                    var credential = !string.IsNullOrWhiteSpace(User.UserId)
                                        ? passwordVault.Retrieve(CREDENTIAL_ACCESSTOKEN_MOBILESERVICES, User.UserId)
                                        : null;
                    return credential != null ? credential.Password : null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                var passwordVault = new PasswordVault();
                passwordVault.Add(new PasswordCredential(CREDENTIAL_ACCESSTOKEN_MOBILESERVICES, User.UserId, value));
            }
        }

        /// <summary>
        /// Logs the user in to Mobile Services using SSO and then retrieves and returns the users UserProfile. 
        /// If a UserProfile does not exist then one is created.
        /// </summary>
        /// <returns>The current Mobile Services user's UserProfile</returns>
        public async Task<UserProfile> LoginAndRetrieveUserProfile()
        {
            if (string.IsNullOrEmpty(User.MobileServiceAuthenticationToken))
            {
                var accessTokenIsCurrent = await AccessTokenCouldBeMadeCurrent();
                if (!accessTokenIsCurrent)
                    return null;
            }

            var userProfileTable = mobileService.GetTable<UserProfile>();
            var userProfiles = await userProfileTable.ToEnumerableAsync();
            var userProfile = userProfiles.SingleOrDefault();

            if (userProfile != null)
                return userProfile;

            userProfile = new UserProfile { UserId = User.UserId };
            await userProfileTable.InsertAsync(userProfile);
            return userProfile;
        }

        public void InitialseWithoutLogin()
        {
            mobileService.CurrentUser = new MobileServiceUser(User.UserId)
            {
                MobileServiceAuthenticationToken = AccessToken
            };
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Check if the OAuth process was successful, 
        /// retrieve the OAuth response code and swap it for a pair of access & refresh tokens
        /// </summary>
        /// <param name="args">The Windows Phone WebAuthenticationBroker arguments from the completed OAuth process</param>
        /// <returns>True if the data source was authorised and an access token was retrieved</returns>
        public bool FinalizeAuthorization(WebAuthenticationBrokerContinuationEventArgs args)
        {
            mobileService.LoginComplete(args);
            return args.WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success;
        }
#endif

        public async Task<Family> GetFamily(string userId)
        {
            var usersFamily = await mobileService.GetTable<Family>()
                                                 .ToEnumerableAsync();
            return usersFamily.SingleOrDefault();
        }

        public Task<IEnumerable<Baby>> GetBabies(string familyId)
        {
            return mobileService.GetTable<Baby>()
                                .Where(b => b.FamilyId == familyId)
                                .ToEnumerableAsync();
        }

        public Task<Baby> GetBaby(string babyId)
        {
            return mobileService.GetTable<Baby>()
                                .LookupAsync(babyId);
        }

        public Task<IEnumerable<TActivity>> GetActivities<TActivity>(string babyId)
            where TActivity : Activity
        {
            return mobileService.GetTable<TActivity>()
                                .Where(b => b.BabyId == babyId)
                                .ToEnumerableAsync();
        }

        public Task<TActivity> GetActivity<TActivity>(string id)
            where TActivity : Activity
        {
            return mobileService.GetTable<TActivity>()
                                .LookupAsync(id);
        }

        public async Task<Measurement> GetLatestMeasurement(string babyId)
        {
            var latestMeasurement = await mobileService.GetTable<Measurement>()
                                                       .Where(m => m.BabyId == babyId)
                                                       .OrderByDescending(m => m.CreatedAt)
                                                       .Take(1)
                                                       .ToEnumerableAsync();
            return latestMeasurement.Single();
        }

        /// <summary>
        /// Attempts to retrieve the activity schedule for a baby either from the cache
        /// if it is younger than 15min, otherwise from the server
        /// </summary>
        /// <param name="babyId"></param>
        /// <returns></returns>
        public async Task<ActivitySchedule> GetActivitySchedule(string babyId)
        {
            // If the cached schedule is fresh then load it
            if (SettingsViewModel.Current.LastActivityScheduleGeneratedAt >= DateTime.UtcNow.AddMinutes(-15))
            {
                var cacheFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("_activitySchedule.json",
                                                                                          CreationCollisionOption.OpenIfExists);
                var activityScheduleCache = await cacheFile.ReadJsonFileAs<Dictionary<string, ActivitySchedule>>();
                return activityScheduleCache[babyId];
            }
            // otherwise request a new one from the server
            else
            {
                var parameters = new Dictionary<string, string> { { "babyId", babyId } };
                return await mobileService.InvokeApiAsync<ActivitySchedule>("ActivitySchedule",
                                                                            HttpMethod.Get,
                                                                            parameters);
            }
        }

        public async Task<Family> CreateFamily(Family family)
        {
            await mobileService.GetTable<Family>()
                               .InsertAsync(family);
            return family;
        }

        public async Task<Baby> CreateBaby(Baby baby)
        {
            await mobileService.GetTable<Baby>()
                               .InsertAsync(baby);
            return baby;
        }

        public async Task<TActivity> CreateActivity<TActivity>(TActivity activity)
            where TActivity : Activity
        {
            await mobileService.GetTable<TActivity>()
                               .InsertAsync(activity);
            return activity;
        }

        public async Task<UserProfile> UpdateUserProfile(UserProfile userProfile)
        {
            await mobileService.GetTable<UserProfile>()
                               .UpdateAsync(userProfile);
            return userProfile;
        }

        public async Task<TActivity> UpdateActivity<TActivity>(TActivity activity)
            where TActivity : Activity
        {
            await mobileService.GetTable<TActivity>()
                               .UpdateAsync(activity);
            return activity;
        }

        public async Task DeleteActivity<TActivity>(TActivity activity)
            where TActivity : Activity
        {
            await mobileService.GetTable<TActivity>()
                               .DeleteAsync(activity);
        }

        /// <summary>
        /// Logs the user in to the Mobile Services instance and stores the returned access token
        /// </summary>
        /// <param name="allowAccessTokenRefresh"></param>
        /// <returns>True if the access token could be refreshed</returns>
        private async Task<bool> AccessTokenCouldBeMadeCurrent()
        {
            try
            {
                User = await mobileService.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount, true);
            }
            catch (Exception)
            {
                // TODO - Log exception
                return false;
            }

            SettingsViewModel.Current.MobileServicesUserId = User.UserId;
            AccessToken = User.MobileServiceAuthenticationToken;
            return true;
        }
    }
}