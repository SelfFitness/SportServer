using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SportServer.Models
{
    public class Plan
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Difficulty { get; set; }

        public int MaxDifficulty { get; set; }

        public IList<ExercisePart> ExerciseParts { get; set; }
    }
}
