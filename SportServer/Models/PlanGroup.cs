namespace SportServer.Models
{
    public class PlanGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Plan> Plans { get; set; }
    }
}
