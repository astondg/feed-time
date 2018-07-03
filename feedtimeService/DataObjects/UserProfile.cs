namespace feedtimeService.DataObjects
{
    using Microsoft.WindowsAzure.Mobile.Service;
    using Newtonsoft.Json;

    public class UserProfile : EntityData
    {
        public string UserId { get; set; }
        public string FamilyId { get; set; }
        [JsonIgnore]
        public virtual Family Family {get; set;}
    }
}