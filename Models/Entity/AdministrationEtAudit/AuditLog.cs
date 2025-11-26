using EcoleApp.Models.Entity.Auth;

namespace EcoleApp.Models.Entity.AdministrationEtAudit
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        public string Action { get; set; } = string.Empty;
        public DateTime DateAction { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } = string.Empty;
    }
}
