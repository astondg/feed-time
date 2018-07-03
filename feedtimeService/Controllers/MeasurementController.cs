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
    public class MeasurementController : TableController<Measurement>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            feedtimeContext context = new feedtimeContext();
            DomainManager = new EntityDomainManager<Measurement>(context, Request, Services, enableSoftDelete: true);
        }

        // GET tables/Measurement
        public IQueryable<Measurement> GetAllMeasurement()
        {
            return Query(); 
        }

        // GET tables/Measurement/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Measurement> GetMeasurement(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Measurement/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Measurement> PatchMeasurement(string id, Delta<Measurement> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Measurement
        public async Task<IHttpActionResult> PostMeasurement(Measurement item)
        {
            Measurement current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Measurement/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteMeasurement(string id)
        {
             return DeleteAsync(id);
        }

    }
}