namespace FeedTime.DataModel
{
    using System;

    public abstract class Entity
    {
        public string Id { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}