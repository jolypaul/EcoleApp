using System.ComponentModel.DataAnnotations;

namespace EcoleApp.ViewModels.Admin
{
    public class ChangeRoleViewModel
    {
        [Required]
        public string UtilisateurId { get; set; } = string.Empty;

        [Required]
        public int NouveauRoleId { get; set; }
    }
}
