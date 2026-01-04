using EcoleApp.Models.Entity.Auth;
using System.ComponentModel.DataAnnotations;

namespace EcoleApp.ViewModels.Admin
{
    public class CreateUserViewModel
    {
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MotDePasse { get; set; } = string.Empty;

        [Required]
        public string RoleId { get; set; } = string.Empty;

        public List<Role> Roles { get; set; } = new();
    }
}
