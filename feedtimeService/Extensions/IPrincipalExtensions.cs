using System.Security.Principal;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace feedtimeService.Extensions
{
    public static class IPrincipalExtensions
    {
        public static string GetId(this IPrincipal user)
        {
            var serviceUser = user as ServiceUser;
            return serviceUser == null ? string.Empty : serviceUser.Id;
        }
    }
}