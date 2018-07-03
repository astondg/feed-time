namespace feedtimeService.DataObjects
{
    using System;
    using Microsoft.WindowsAzure.Mobile.Service;
    using Newtonsoft.Json;

    public abstract class Activity : EntityData
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string Notes { get; set; }
        public int? HowBabyFelt { get; set; }
        public int? HowParentFelt { get; set; }
        public string BabyId { get; set; }
        [JsonIgnore]
        public virtual Baby Baby { get; set; }
    }
}