namespace feedtimeService.Controllers
{
    using System.Linq;
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
    public class FamilyController : TableController<Family>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            feedtimeContext context = new feedtimeContext();
            DomainManager = new EntityDomainManager<Family>(context, Request, Services, enableSoftDelete: true);
        }

        // GET tables/Family
        public IQueryable<Family> GetAllFamily()
        {
            string userId = User.GetId();
            return Query().Where(family => family.UserProfiles.Any(up => up.UserId == userId)); 
        }

        // GET tables/Family/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Family> GetFamily(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Family/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Family> PatchFamily(string id, Delta<Family> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Family
        public async Task<IHttpActionResult> PostFamily(Family item)
        {
            Family current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Family/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteFamily(string id)
        {
             return DeleteAsync(id);
        }

    }
}