namespace FeedTime.DataModel
{
    using System;

    public abstract class Activity : Entity
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string Notes { get; set; }
        public string BabyId { get; set; }
        public Feeling? HowBabyFelt { get; set; }
        public Feeling? HowParentFelt { get; set; }
    }
}