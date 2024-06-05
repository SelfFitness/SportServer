namespace SportServer.Models.Viewmodels
{
    public class ChangePasswordViewmodel
    {
        public int Id { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
