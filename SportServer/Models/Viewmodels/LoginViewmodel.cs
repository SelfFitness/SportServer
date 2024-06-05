using System.ComponentModel.DataAnnotations;

namespace SportServer.Models.Viewmodels
{
    public class LoginViewmodel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
