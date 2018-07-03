namespace FeedTime.DataModel
{
    public class Measurement : Entity
    {
        public double? Length { get; set; }
        public double? Weight { get; set; }
        public string BabyId { get; set; }
    }
}