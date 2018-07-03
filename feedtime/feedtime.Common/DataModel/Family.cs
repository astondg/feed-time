namespace FeedTime.Common.DataModel
{
    using System;
    using Microsoft.WindowsAzure.MobileServices;
    using Newtonsoft.Json;

    public sealed class Family
    {
        [JsonProperty("id")]
        public string Id { get; set; }
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