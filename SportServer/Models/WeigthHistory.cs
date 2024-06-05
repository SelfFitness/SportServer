using SportServer.Data;
using System.Text.Json.Serialization;

namespace SportServer.Models
{
    public class WeigthHistory
    {
        public int Id { get; set; }

        public double? Weigth { get; set; }

        public DateTime Date { get; set; }

        [JsonIgnore]
        public AppUser AppUser { get; set; }
    }
}
