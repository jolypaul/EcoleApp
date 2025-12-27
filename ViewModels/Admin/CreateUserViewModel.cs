using EcoleApp.Models.Entity.Auth;
using System.ComponentModel.DataAnnotations;

namespace EcoleApp.ViewModels.Admin
{
    public class CreateUserViewModel
    {
        [Required]
        public string Nom { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string MotDePasse { get; set; }

        [Required]
        public int RoleId { get; set; }

        public List<Role> Roles { get; set; }
    }

}
