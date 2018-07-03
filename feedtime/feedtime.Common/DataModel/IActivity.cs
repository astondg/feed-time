namespace FeedTime.Common.DataModel
{
    using System;

    public interface IActivity
    {
        string Id { get; set; }
        DateTimeOffset StartTime { get; set; }
        DateTimeOffset? EndTime { get; set; }
        string Notes { get; set; }
        string BabyId { get; set; }
        int? HowBabyFelt { get; set; }
        int? HowParentFelt { get; set; }
        //DateTimeOffset? CreatedAt { get; set; }
        //DateTimeOffset? UpdatedAt { get; set; }
    }
}