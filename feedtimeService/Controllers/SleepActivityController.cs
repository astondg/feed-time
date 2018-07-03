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
    public class SleepActivityController : TableController<SleepActivity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            feedtimeContext context = new feedtimeContext();
            DomainManager = new EntityDomainManager<SleepActivity>(context, Request, Services, enableSoftDelete: true);
        }

        // GET tables/SleepActivity
        public IQueryable<SleepActivity> GetAllSleepActivity()
        {
            return Query(); 
        }

        // GET tables/SleepActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<SleepActivity> GetSleepActivity(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/SleepActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<SleepActivity> PatchSleepActivity(string id, Delta<SleepActivity> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/SleepActivity
        public async Task<IHttpActionResult> PostSleepActivity(SleepActivity item)
        {
            SleepActivity current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/SleepActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteSleepActivity(string id)
        {
             return DeleteAsync(id);
        }

    }
}