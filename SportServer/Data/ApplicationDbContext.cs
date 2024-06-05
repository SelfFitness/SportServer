using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportServer.Models;

namespace SportServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<ExercisePart> ExerciseParts { get; set; }

        public DbSet<Plan> Plans { get; set; }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<PlanGroup> PlanGroups { get; set; }

        public DbSet<WeigthHistory> WeigthHistory { get; set; }

        public DbSet<TrainHistory> TrainHistory { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
