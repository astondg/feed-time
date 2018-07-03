using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using feedtimeService.DataObjects;
using feedtimeService.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace feedtimeService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class ChangeActivityController : TableController<ChangeActivity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            feedtimeContext context = new feedtimeContext();
            DomainManager = new EntityDomainManager<ChangeActivity>(context, Request, Services, enableSoftDelete: true);
        }

        // GET tables/ChangeActivity
        public IQueryable<ChangeActivity> GetAllChangeActivity()
        {
            return Query(); 
        }

        // GET tables/ChangeActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<ChangeActivity> GetChangeActivity(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/ChangeActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<ChangeActivity> PatchChangeActivity(string id, Delta<ChangeActivity> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/ChangeActivity
        public async Task<IHttpActionResult> PostChangeActivity(ChangeActivity item)
        {
            ChangeActivity current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/ChangeActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteChangeActivity(string id)
        {
             return DeleteAsync(id);
        }

    }
}