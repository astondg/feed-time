namespace feedtimeService.DataObjects
{
    using System;
    using Microsoft.WindowsAzure.Mobile.Service;
    using Newtonsoft.Json;

    public class Baby : EntityData
    {
        public string GivenName { get; set; }
        public string AdditionalName { get; set; }
        public string FamilyName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string FamilyId { get; set; }
        [JsonIgnore]
        public virtual Family Family { get; set; }
    }
}