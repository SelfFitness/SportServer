using SportServer.Data;
using System.Text.Json.Serialization;

namespace SportServer.Models
{
    public class TrainHistory
    {
        public int Id { get; set; }

        public int PlanId { get; set; }

        public DateTime Date { get; set; }

        [JsonIgnore]
        public Plan Plan { get; set; }

        [JsonIgnore]
        public AppUser AppUser { get; set; }
    }
}
