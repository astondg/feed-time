namespace FeedTime.DataModel
{
    using System;

    public class Baby : Entity
    {
        public string GivenName { get; set; }
        public string AdditionalName { get; set; }
        public string FamilyName { get; set; }
        public string FamilyId { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public Gender Gender { get; set; }
    }
}