using System.ComponentModel.DataAnnotations;

namespace MDSApi.Authentication
{
    public class LoginModel
    {
        public string Domain { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
