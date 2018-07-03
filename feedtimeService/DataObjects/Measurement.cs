namespace feedtimeService.DataObjects
{
    using Microsoft.WindowsAzure.Mobile.Service;
    using Newtonsoft.Json;

    public class Measurement : EntityData
    {
        public double? Length { get; set; }
        public double? Weight { get; set; }
        public string BabyId { get; set; }
        [JsonIgnore]
        public virtual Baby Baby { get; set; }
    }
}