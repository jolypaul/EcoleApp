using System.ComponentModel.DataAnnotations;

namespace EcoleApp.ViewModels.Admin
{
    public class ChangeRoleViewModel
    {
        [Required]
        public string UtilisateurId { get; set; } = string.Empty;

        [Required]
        public string NouveauRoleId { get; set; } = string.Empty;
    }
}
