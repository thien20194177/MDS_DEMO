using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MDSApi.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string Domain { get; set; }
    }
}
