using Microsoft.AspNetCore.Identity;
using SportServer.Models;

namespace SportServer.Data
{
    public class AppUser : IdentityUser
    {
        public double Heigth { get; set; }

        public double Weigth { get; set; }

        public List<WeigthHistory> WeigthHistory { get; set; }

        public List<TrainHistory> TrainHistory { get; set; }
    }
}
