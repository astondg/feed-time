namespace feedtimeService.DataObjects
{
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Mobile.Service;
    using Newtonsoft.Json;

    public class Family : EntityData
    {
        [JsonIgnore]
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
        [JsonIgnore]
        public virtual ICollection<Baby> Babies { get; set; }
    }
}