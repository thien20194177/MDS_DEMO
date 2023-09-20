using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MDSApi.ViewModel
{
    public class UserEditModel
    {
        [Key]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
