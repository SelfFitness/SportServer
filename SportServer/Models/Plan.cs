namespace SportServer.Models
{
    public class Plan
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Difficulty { get; set; }

        public int MaxDifficulty { get; set; }

        public IList<ExercisePart> Exercises { get; set; }
    }
}
