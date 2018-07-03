namespace FeedTime.Common.DataModel
{
    using System;
    using Microsoft.WindowsAzure.MobileServices;
    using Newtonsoft.Json;

    public sealed class FeedActivity : IActivity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string Notes { get; set; }
        public string BabyId { get; set; }
        public int? HowBabyFelt { get; set; }
        public int? HowParentFelt { get; set; }
        public int? AmountConsumed { get; set; }
        public int? FeedingSide { get; set; }
        [CreatedAt]
        [JsonProperty("__createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }
        [UpdatedAt]
        [JsonProperty("__updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }
        [Version]
        [JsonProperty("__version")]
        public string Version { get; set; }
        [Deleted]
        [JsonProperty("__deleted")]
        public bool IsDeleted { get; set; }
    }
}