namespace SportServer.Models
{
    public class ExercisePart
    {
        public int Id { get; set; }

        public Exercise Exercise { get; set; }

        public TimeSpan? Duration { get; set; }

        public int? Count { get; set; }
    }
}
