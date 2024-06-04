using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportServer.Models;

namespace SportServer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<ExercisePart> ExerciseParts { get; set; }

        public DbSet<Plan> Plans { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
