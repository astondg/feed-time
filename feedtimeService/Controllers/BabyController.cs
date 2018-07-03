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
    public class BabyController : TableController<Baby>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            feedtimeContext context = new feedtimeContext();
            DomainManager = new EntityDomainManager<Baby>(context, Request, Services, enableSoftDelete: true);
        }

        // GET tables/Baby
        public IQueryable<Baby> GetAllBaby()
        {
            string userId = User.GetId();
            // Only return babies in the users family
            return Query().Where(b => b.Family.UserProfiles.Any(up => up.UserId == userId));
        }

        // GET tables/Baby/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Baby> GetBaby(string id)
        {
            string userId = User.GetId();
            var baby = Lookup(id);
            // Ensure that the baby being retrieved belongs to the users family
            if (baby.Queryable.All(b => b.Family.UserProfiles.Any(up => up.UserId == userId)))
                return baby;
            else
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // PATCH tables/Baby/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Baby> PatchBaby(string id, Delta<Baby> patch)
        {
            // Ensure that the baby being updated belongs to the users family
            using (var context = new feedtimeContext())
            {
                string userId = User.GetId();
                var baby = context.Set<Baby>()
                                  .SingleOrDefault(b => b.Id == id && b.Family.UserProfiles.Any(up => up.UserId == userId));
                if (baby == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return UpdateAsync(id, patch);
        }

        // POST tables/Baby
        public async Task<IHttpActionResult> PostBaby(Baby item)
        {
            // Ensure the baby is only inserted into the users family
            using (var context = new feedtimeContext())
            {
                string userId = User.GetId();
                var userProfile = context.Set<UserProfile>()
                                         .Single(up => up.UserId == userId);
                item.FamilyId = userProfile.FamilyId;
            }

            Baby current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Baby/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteBaby(string id)
        {
            // Ensure that the baby being deleted belongs to the users family
            using (var context = new feedtimeContext())
            {
                string userId = User.GetId();
                var baby = context.Set<Baby>()
                                  .SingleOrDefault(b => b.Id == id && b.Family.UserProfiles.Any(up => up.UserId == userId));
                if (baby == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return DeleteAsync(id);
        }
    }
}