namespace feedtimeService.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.OData;
    using feedtimeService.DataObjects;
    using feedtimeService.Extensions;
    using feedtimeService.Models;
    using Microsoft.WindowsAzure.Mobile.Service;
    using Microsoft.WindowsAzure.Mobile.Service.Security;

    [AuthorizeLevel(AuthorizationLevel.User)]
    public class UserProfileController : TableController<UserProfile>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            feedtimeContext context = new feedtimeContext();
            DomainManager = new EntityDomainManager<UserProfile>(context, Request, Services, enableSoftDelete: true);
        }

        // GET tables/UserProfile
        public IQueryable<UserProfile> GetAllUserProfile()
        {
            string userId = User.GetId();
            // Only return UserProfiles that belong to the current user
            return Query().Where(up => up.UserId == userId); 
        }

        // GET tables/UserProfile/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<UserProfile> GetUserProfile(string id)
        {
            // Ensure that the user profile being retrieved belongs to the current user
            string userId = User.GetId();
            var userProfile = Lookup(id);
            if (userProfile.Queryable.All(up => up.UserId == userId))
                return userProfile;
            else
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // PATCH tables/UserProfile/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<UserProfile> PatchUserProfile(string id, Delta<UserProfile> patch)
        {
            // Ensure that the user profile being updated belongs to the current user
            using (var context = new feedtimeContext())
            {
                string userId = User.GetId();
                var userProfile = context.Set<UserProfile>()
                                         .SingleOrDefault(up => up.UserId == userId);
                if (userProfile == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

             return UpdateAsync(id, patch);
        }

        // POST tables/UserProfile
        public async Task<IHttpActionResult> PostUserProfile(UserProfile item)
        {
            // Ensure that a user profile is only created for the current user
            item.UserId = User.GetId();
            UserProfile current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/UserProfile/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteUserProfile(string id)
        {
            // Ensure that the user profile being deleted belongs to the current user
            using (var context = new feedtimeContext())
            {
                string userId = User.GetId();
                var userProfile = context.Set<UserProfile>()
                                         .SingleOrDefault(up => up.UserId == userId);
                if (userProfile == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return DeleteAsync(id);
        }
    }
}