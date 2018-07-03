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
    public class FeedActivityController : TableController<FeedActivity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            feedtimeContext context = new feedtimeContext();
            DomainManager = new EntityDomainManager<FeedActivity>(context, Request, Services, enableSoftDelete: true);
        }

        // GET tables/FeedActivity
        public IQueryable<FeedActivity> GetAllFeedActivity()
        {
            return Query(); 
        }

        // GET tables/FeedActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<FeedActivity> GetFeedActivity(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/FeedActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<FeedActivity> PatchFeedActivity(string id, Delta<FeedActivity> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/FeedActivity
        public async Task<IHttpActionResult> PostFeedActivity(FeedActivity item)
        {
            FeedActivity current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/FeedActivity/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteFeedActivity(string id)
        {
            return DeleteAsync(id);
        }

    }
}