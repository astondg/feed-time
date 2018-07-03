namespace FeedTime.Common.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FeedTime.Common.Extensions;
    using FeedTime.Common.Strings;
    using Microsoft.WindowsAzure.MobileServices;
    using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
    using Microsoft.WindowsAzure.MobileServices.Sync;
    using Newtonsoft.Json.Linq;
    using Windows.ApplicationModel.Activation;
    using Windows.Foundation;
    using Windows.Networking.Connectivity;
    using Windows.Security.Authentication.OnlineId;
    using Windows.Security.Authentication.Web;
    using Windows.Security.Credentials;
    using Windows.Storage;

    public sealed class MobileServicesDataSource
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

        private string currentUserId;
        private bool refreshActivitySchedule;
        private bool refreshDataTrends;
        private bool internetAccessIsAvailable;
        private bool isRegisteredForNetworkStatusChanges;
        private bool isAuthenticating;
        private CacheRetriever cacheRetriever;
        private IAccessTokenRetriever accessTokenRetriever;
        private ILogger logger;

        public MobileServicesDataSource(string userId, IAccessTokenRetriever accessTokenRetriever, ILogger logger)
        {
            currentUserId = userId;
            refreshActivitySchedule = false;
            refreshDataTrends = false;
            internetAccessIsAvailable = false;
            isRegisteredForNetworkStatusChanges = false;
            isAuthenticating = false;
            cacheRetriever = new CacheRetriever();
            this.accessTokenRetriever = accessTokenRetriever;
            this.accessTokenRetriever.LoginCompleted += accessTokenRetriever_LoginCompleted;
            this.logger = logger;
        }

        public string CurrentUserId { get { return currentUserId; } }
        public bool IsAuthenticating { get { return isAuthenticating; } }

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
                    var credential = !string.IsNullOrWhiteSpace(currentUserId)
                                        ? passwordVault.Retrieve(CREDENTIAL_ACCESSTOKEN_MOBILESERVICES,
                                                                 currentUserId)
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
                if (value == null && !string.IsNullOrWhiteSpace(currentUserId))
                {
                    try
                    {
                        var credential = passwordVault.Retrieve(CREDENTIAL_ACCESSTOKEN_MOBILESERVICES,
                                                                currentUserId);
                        if (credential != null)
                            passwordVault.Remove(credential);
                    }
                    catch (Exception)
                    { /* The credential doesn't exist - all ok */ }
                }
                else
                {
                    passwordVault.Add(new PasswordCredential(CREDENTIAL_ACCESSTOKEN_MOBILESERVICES,
                                                             currentUserId,
                                                             value));
                }
            }
        }

        public IAsyncOperation<bool> Login(bool attemptAuthorisation, bool initialiseForOfflineAccess)
        {
            try
            {
                isAuthenticating = true;
                // Check for Internet connectivity and register for future connectivity changes
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                internetAccessIsAvailable = connectionProfile != null
                                            && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
                if (!isRegisteredForNetworkStatusChanges)
                    NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

                return AccessTokenCouldBeMadeCurrent(attemptAuthorisation, initialiseForOfflineAccess).AsAsyncOperation();
            }
            finally
            {
                isAuthenticating = false;
            }
        }

        public bool FinaliseLogin()
        {
            try
            {
                isAuthenticating = true;
                return accessTokenRetriever.CompleteLogin(mobileService);
            }
            finally
            {
                isAuthenticating = false;
            }
        }

        public void Logout()
        {
            mobileService.Logout();
            AccessToken = null;
            currentUserId = null;
        }

        public IAsyncAction PushLocalChangesToServer()
        {
            return PushLocalChangesToServerIfAvilable().AsAsyncAction();
        }

        /// <summary>
        /// Logs the user in to Mobile Services using SSO and then retrieves and returns the users UserProfile. 
        /// If a UserProfile does not exist then one is created.
        /// </summary>
        /// <returns>The current Mobile Services user's UserProfile</returns>
        public IAsyncOperation<UserProfile> RetrieveUserProfile()
        {
            return RetrieveUserProfileInternal().AsAsyncOperation();
        }
        private async Task<UserProfile> RetrieveUserProfileInternal()
        {
            var userProfileTable = await GetInitialisedSyncTable<UserProfile>(currentUserId);
            var userProfiles = await userProfileTable.Where(up => up.UserId == currentUserId)
                                                     .ToEnumerableAsync();
            var userProfile = userProfiles.SingleOrDefault();

            if (userProfile != null)
                return userProfile;

            userProfile = new UserProfile { UserId = currentUserId };
            await userProfileTable.InsertAsync(userProfile);
            if (internetAccessIsAvailable)
                await mobileService.SyncContext.PushAsync();
            return userProfile;
        }

        public IAsyncOperation<Family> GetFamily(string userId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    var familyTable = await GetInitialisedSyncTable<Family>(userId);
                    var usersFamily = await familyTable.ToEnumerableAsync();
                    return usersFamily.SingleOrDefault();
                }).AsAsyncOperation();
        }

        public IAsyncOperation<IEnumerable<Baby>> GetBabies(string familyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    var babyTable = await GetInitialisedSyncTable<Baby>(familyId, baby => baby.FamilyId == familyId);
                    return await babyTable.Where(b => b.FamilyId == familyId)
                                          .ToEnumerableAsync();
                }).AsAsyncOperation();
        }

        public IAsyncOperation<Baby> GetBaby(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    var babyTable = await GetInitialisedSyncTable<Baby>(babyId);
                    return await babyTable.LookupAsync(babyId);
                }).AsAsyncOperation();
        }

        public IAsyncOperation<IEnumerable<FeedActivity>> GetFeedActivities(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var activityTable = await GetInitialisedSyncTable<FeedActivity>(babyId, fa => fa.BabyId == babyId);
                return await activityTable.Where(fa => fa.BabyId == babyId)
                                          .ToEnumerableAsync();
            }).AsAsyncOperation();
        }

        public IAsyncOperation<FeedActivity> GetFeedActivity(string id)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var activityTable = await GetInitialisedSyncTable<FeedActivity>(id);
                return await activityTable.LookupAsync(id);
            }).AsAsyncOperation();
        }

        public IAsyncOperation<IEnumerable<SleepActivity>> GetSleepActivities(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var activityTable = await GetInitialisedSyncTable<SleepActivity>(babyId, sa => sa.BabyId == babyId);
                return await activityTable.Where(sa => sa.BabyId == babyId)
                                          .ToEnumerableAsync();
            }).AsAsyncOperation();
        }

        public IAsyncOperation<SleepActivity> GetSleepActivity(string id)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var activityTable = await GetInitialisedSyncTable<SleepActivity>(id);
                return await activityTable.LookupAsync(id);
            }).AsAsyncOperation();
        }

        public IAsyncOperation<IEnumerable<ChangeActivity>> GetChangeActivities(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var activityTable = await GetInitialisedSyncTable<ChangeActivity>(babyId, ca => ca.BabyId == babyId);
                return await activityTable.Where(ca => ca.BabyId == babyId)
                                          .ToEnumerableAsync();
            }).AsAsyncOperation();
        }

        public IAsyncOperation<ChangeActivity> GetChangeActivity(string id)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var activityTable = await GetInitialisedSyncTable<ChangeActivity>(id);
                return await activityTable.LookupAsync(id);
            }).AsAsyncOperation();
        }

        public IAsyncOperation<Measurement> GetLatestMeasurement(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var measurementTable = await GetInitialisedSyncTable<Measurement>(babyId, m => m.BabyId == babyId);
                var latestMeasurement = await measurementTable.Where(m => m.BabyId == babyId)
                                                              .OrderByDescending(m => m.CreatedAt)
                                                              .Take(1)
                                                              .ToEnumerableAsync();
                return latestMeasurement.SingleOrDefault();
            }).AsAsyncOperation();
        }

        /// <summary>
        /// Attempts to retrieve the activity schedule for a baby either from the cache
        /// if it is younger than 15min, otherwise from the server
        /// </summary>
        /// <param name="babyId"></param>
        /// <returns></returns>
        public IAsyncOperation<ActivitySchedule> GetActivitySchedule(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var activitySchedule = await cacheRetriever.RetrieveActivitySchedule(babyId);

                // If the cached schedule is fresh then return it
                if (!internetAccessIsAvailable
                    || (!refreshActivitySchedule
                    && activitySchedule != null
                    && activitySchedule.ScheduleGeneratedAt >= DateTimeOffset.Now.AddMinutes(-15)
                    && !AnyActivityHasExpired(activitySchedule)))
                {
                    return activitySchedule;
                }
                // otherwise request a new one from the server
                else
                {
                    var parameters = new Dictionary<string, string> { { "babyId", babyId } };
                    ActivitySchedule latestSchedule;
                    try
                    {
                        latestSchedule = await mobileService.InvokeApiAsync<ActivitySchedule>("ActivitySchedule",
                                                                                              HttpMethod.Get,
                                                                                              parameters);
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                            return null;
                        else
                            throw;
                    }
                    await cacheRetriever.SaveActivitySchedule(latestSchedule, babyId);
                    refreshActivitySchedule = false;
                    return latestSchedule;
                }
            }).AsAsyncOperation();
        }

        public IAsyncOperation<DataTrends> GetDataTrends(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var dataTrends = await cacheRetriever.RetrieveDataTrends(babyId);

                // If the cached trends are fresh then return them
                if (!internetAccessIsAvailable
                    || (!refreshDataTrends
                    && dataTrends != null
                    && dataTrends.TrendsGeneratedAt >= DateTimeOffset.Now.AddMinutes(-30)))
                {
                    return dataTrends;
                }
                // otherwise request a new one from the server
                else
                {
                    var parameters = new Dictionary<string, string> { { "babyId", babyId } };
                    DataTrends latestTrends;
                    try
                    {
                        latestTrends = await mobileService.InvokeApiAsync<DataTrends>("DataTrends",
                                                                                      HttpMethod.Get,
                                                                                      parameters);
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                            return null;
                        else
                            throw;
                    }
                    await cacheRetriever.SaveDataTrends(latestTrends, babyId);
                    refreshDataTrends = false;
                    return latestTrends;
                }
            }).AsAsyncOperation();
        }

        public IAsyncOperation<IEnumerable<IActivity>> GetActivityHistory(string babyId)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                var feedActivityTable = await GetInitialisedSyncTable<FeedActivity>(babyId, a => a.BabyId == babyId);
                var feedHistory = await feedActivityTable.Where(a => a.BabyId == babyId)
                                                         .OrderByDescending(a => a.StartTime)
                                                         .Take(10)
                                                         .ToEnumerableAsync();
                var sleepActivityTable = await GetInitialisedSyncTable<SleepActivity>(babyId, a => a.BabyId == babyId);
                var sleepHistory = await sleepActivityTable.Where(a => a.BabyId == babyId)
                                                           .OrderByDescending(a => a.StartTime)
                                                           .Take(10)
                                                           .ToEnumerableAsync();
                var changeActivityTable = await GetInitialisedSyncTable<ChangeActivity>(babyId, a => a.BabyId == babyId);
                var changeHistory = await changeActivityTable.Where(a => a.BabyId == babyId)
                                                             .OrderByDescending(a => a.StartTime)
                                                             .Take(10)
                                                             .ToEnumerableAsync();
                return feedHistory.Cast<IActivity>()
                                    .Union(sleepHistory.Cast<IActivity>())
                                    .Union(changeHistory.Cast<IActivity>())
                                    .OrderByDescending(a => a.StartTime)
                                    .AsEnumerable();
                }).AsAsyncOperation();
        }

        public IAsyncOperation<Family> CreateFamily(Family family)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    await mobileService.GetSyncTable<Family>()
                                       .InsertAsync(family);
                    if (internetAccessIsAvailable)
                        await mobileService.SyncContext.PushAsync();
                    return family;
                }).AsAsyncOperation();
        }

        public IAsyncOperation<Baby> CreateBaby(Baby baby)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    await mobileService.GetSyncTable<Baby>()
                                        .InsertAsync(baby);
                    if (internetAccessIsAvailable)
                        await mobileService.SyncContext.PushAsync();
                    return baby;
                }).AsAsyncOperation();
        }

        public IAsyncOperation<FeedActivity> CreateFeedActivity(FeedActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    await mobileService.GetSyncTable<FeedActivity>()
                                       .InsertAsync(activity);
                    if (internetAccessIsAvailable)
                        await mobileService.SyncContext.PushAsync();
                    ForceReportsRefresh();
                    return activity;
                }).AsAsyncOperation();
        }

        public IAsyncOperation<SleepActivity> CreateSleepActivity(SleepActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                await mobileService.GetSyncTable<SleepActivity>()
                                   .InsertAsync(activity);
                if (internetAccessIsAvailable)
                    await mobileService.SyncContext.PushAsync();
                ForceReportsRefresh();
                return activity;
            }).AsAsyncOperation();
        }

        public IAsyncOperation<ChangeActivity> CreateChangeActivity(ChangeActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                await mobileService.GetSyncTable<ChangeActivity>()
                                   .InsertAsync(activity);
                if (internetAccessIsAvailable)
                    await mobileService.SyncContext.PushAsync();
                ForceReportsRefresh();
                return activity;
            }).AsAsyncOperation();
        }

        public IAsyncOperation<Measurement> CreateMeasurement(Measurement measurement)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    await mobileService.GetSyncTable<Measurement>()
                                       .InsertAsync(measurement);
                    if (internetAccessIsAvailable)
                        await mobileService.SyncContext.PushAsync();
                    ForceReportsRefresh();
                    return measurement;
                }).AsAsyncOperation();
        }

        public IAsyncOperation<UserProfile> UpdateUserProfile(UserProfile userProfile)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    await mobileService.GetSyncTable<UserProfile>()
                                       .UpdateAsync(userProfile);
                    if (internetAccessIsAvailable)
                        await mobileService.SyncContext.PushAsync();
                    return userProfile;
                }).AsAsyncOperation();
        }

        public IAsyncOperation<Baby> UpdateBaby(Baby baby)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    await mobileService.GetSyncTable<Baby>()
                                       .UpdateAsync(baby);
                    if (internetAccessIsAvailable)
                        await mobileService.SyncContext.PushAsync();
                    return baby;
                }).AsAsyncOperation();
        }

        public IAsyncOperation<FeedActivity> UpdateFeedActivity(FeedActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                await mobileService.GetSyncTable<FeedActivity>()
                                   .UpdateAsync(activity);
                if (internetAccessIsAvailable)
                    await mobileService.SyncContext.PushAsync();
                ForceReportsRefresh();
                return activity;
            }).AsAsyncOperation();
        }

        public IAsyncOperation<SleepActivity> UpdateSleepActivity(SleepActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                await mobileService.GetSyncTable<SleepActivity>()
                                   .UpdateAsync(activity);
                if (internetAccessIsAvailable)
                    await mobileService.SyncContext.PushAsync();
                ForceReportsRefresh();
                return activity;
            }).AsAsyncOperation();
        }

        public IAsyncAction DeleteFeedActivity(FeedActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                await mobileService.GetSyncTable<FeedActivity>()
                                   .DeleteAsync(activity);
                if (internetAccessIsAvailable)
                    await mobileService.SyncContext.PushAsync();
                ForceReportsRefresh();
            }).AsAsyncAction();
        }

        public IAsyncAction DeleteSleepActivity(SleepActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                await mobileService.GetSyncTable<SleepActivity>()
                                   .DeleteAsync(activity);
                if (internetAccessIsAvailable)
                    await mobileService.SyncContext.PushAsync();
                ForceReportsRefresh();
            }).AsAsyncAction();
        }

        public IAsyncAction DeleteChangeActivity(ChangeActivity activity)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
                {
                    await mobileService.GetSyncTable<ChangeActivity>()
                                       .DeleteAsync(activity);
                    if (internetAccessIsAvailable)
                        await mobileService.SyncContext.PushAsync();
                    ForceReportsRefresh();
                }).AsAsyncAction();
        }

        public IAsyncAction DeleteMeasurement(Measurement measurement)
        {
            return PerformOperationAndRetryIfUnauthorized(async () =>
            {
                await mobileService.GetSyncTable<Measurement>()
                                   .DeleteAsync(measurement);
                if (internetAccessIsAvailable)
                    await mobileService.SyncContext.PushAsync();
                ForceReportsRefresh();
            }).AsAsyncAction();
        }

        private async Task<T> PerformOperationAndRetryIfUnauthorized<T>(Func<Task<T>> operation)
        {
            MobileServicePushFailedException pushFailedException = null;

            try
            {
                return await operation();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (ex.Response.StatusCode != HttpStatusCode.Unauthorized)
                    throw;
                AccessToken = null;
            }
            catch (MobileServicePushFailedException ex)
            {
                pushFailedException = ex;
            }

            if (pushFailedException != null)
            {
                string errorMessages = pushFailedException.PushResult
                                                          .Errors
                                                          .Aggregate(new StringBuilder(),
                                                                     (s, a) => s.AppendLine(a.RawResult),
                                                                     s => s.ToString());
                await logger.LogExceptionAsync(pushFailedException, Constants.LOGGING_DATAKEY_PUSHERRORS, errorMessages);
                throw pushFailedException;
            }

            var accessTokenUpdated = await AccessTokenCouldBeMadeCurrent();
            if (accessTokenUpdated)
                return await operation();
            else
                throw new UnauthorizedAccessException();
        }

        private async Task PerformOperationAndRetryIfUnauthorized(Func<Task> operation)
        {
            MobileServicePushFailedException pushFailedException = null;

            try
            {
                await operation();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (ex.Response.StatusCode != HttpStatusCode.Unauthorized)
                    throw;
                AccessToken = null;
            }
            catch (MobileServicePushFailedException ex)
            {
                pushFailedException = ex;
            }

            if (pushFailedException != null)
            {
                string errorMessages = pushFailedException.PushResult
                                                          .Errors
                                                          .Aggregate(new StringBuilder(),
                                                                     (s, a) => s.AppendLine(a.RawResult),
                                                                     s => s.ToString());
                await logger.LogExceptionAsync(pushFailedException, Constants.LOGGING_DATAKEY_PUSHERRORS, errorMessages);
                throw pushFailedException;
            }

            var accessTokenUpdated = await AccessTokenCouldBeMadeCurrent();
            if (accessTokenUpdated)
                await operation();
            else
                throw new UnauthorizedAccessException();
        }

        /// <summary>
        /// Logs the user in to the Mobile Services instance and stores the returned access token
        /// </summary>
        /// <returns>True if the access token could be refreshed</returns>
        private async Task<bool> AccessTokenCouldBeMadeCurrent(bool attemptAuthorisation = true, bool initialiseForOfflineAccess = true)
        {
            if (!string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrEmpty(currentUserId))
            {
                mobileService.CurrentUser = new MobileServiceUser(currentUserId)
                {
                    MobileServiceAuthenticationToken = AccessToken
                };
            }
            else if (!attemptAuthorisation)
            {
                return false;
            }
            else
            {
                await accessTokenRetriever.RefreshUserIdAndAccessToken(mobileService);
                if (string.IsNullOrWhiteSpace(accessTokenRetriever.AccessToken)) return false;

                currentUserId = mobileService.CurrentUser.UserId = accessTokenRetriever.UserId;
                AccessToken = mobileService.CurrentUser.MobileServiceAuthenticationToken = accessTokenRetriever.AccessToken;
            }

            if (initialiseForOfflineAccess && !mobileService.SyncContext.IsInitialized)
                await InitialiseMobileServicesLocalSyncContext();

            return true;
        }

        private async Task InitialiseMobileServicesLocalSyncContext()
        {
            var store = new MobileServiceSQLiteStore(Constants.LOCALSTORAGE_SQLLITE_FILENAME);
            store.DefineTable<Baby>();
            store.DefineTable<ChangeActivity>();
            store.DefineTable<Family>();
            store.DefineTable<FeedActivity>();
            store.DefineTable<Measurement>();
            store.DefineTable<SleepActivity>();
            store.DefineTable<UserProfile>();
            await mobileService.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
        }

        private async Task SyncAllLocalTablesWithServer()
        {
            await mobileService.GetSyncTable<Baby>()
                               .PullAsync("FullSync", null);
            await mobileService.GetSyncTable<ChangeActivity>()
                               .PullAsync("FullSync", null);
            await mobileService.GetSyncTable<Family>()
                               .PullAsync("FullSync", null);
            await mobileService.GetSyncTable<FeedActivity>()
                               .PullAsync("FullSync", null);
            await mobileService.GetSyncTable<Measurement>()
                               .PullAsync("FullSync", null);
            await mobileService.GetSyncTable<SleepActivity>()
                               .PullAsync("FullSync", null);
            await mobileService.GetSyncTable<UserProfile>()
                               .PullAsync("FullSync", null);
        }

        private bool AnyActivityHasExpired(ActivitySchedule activitySchedule)
        {
            return activitySchedule.NextChangeDueAt <= DateTimeOffset.Now
                    || activitySchedule.NextFeedDueAt <= DateTimeOffset.Now
                    || activitySchedule.NextSleepDueAt <= DateTimeOffset.Now;
        }

        private void ForceReportsRefresh()
        {
            cacheRetriever.ExpireCache();
            refreshActivitySchedule = true;
            refreshDataTrends = true;
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            internetAccessIsAvailable = connectionProfile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess;
        }

        private async Task<IMobileServiceSyncTable<T>> GetInitialisedSyncTable<T>(string key, Expression<Func<T, bool>> query = null)
        {
            MobileServicePushFailedException pushFailedException = null;
            var table = mobileService.GetSyncTable<T>();
            var mobileServicesQuery = query == null
                                        ? null
                                        : table.Where(query);
            try
            {
                if (internetAccessIsAvailable)
                    await table.PullAsync(key, mobileServicesQuery ?? table.CreateQuery());
            }
            catch (MobileServicePushFailedException ex)
            {
                pushFailedException = ex;
            }

            if (pushFailedException != null)
            {
                string errorMessages = pushFailedException.PushResult
                                                          .Errors
                                                          .Aggregate(new StringBuilder(),
                                                                     (s, a) => s.AppendLine(a.RawResult),
                                                                     s => s.ToString());
                await logger.LogExceptionAsync(pushFailedException, Constants.LOGGING_DATAKEY_PUSHERRORS, errorMessages);
                throw pushFailedException;
            }

            return table;
        }

        private async Task PushLocalChangesToServerIfAvilable()
        {
            if (internetAccessIsAvailable && mobileService.SyncContext.PendingOperations > 0)
                await mobileService.SyncContext
                                   .PushAsync();
        }

        private void accessTokenRetriever_LoginCompleted(IAccessTokenRetriever sender, bool args)
        {
            currentUserId = mobileService.CurrentUser.UserId = accessTokenRetriever.UserId;
            AccessToken = mobileService.CurrentUser.MobileServiceAuthenticationToken = accessTokenRetriever.AccessToken;
        }
    }
}