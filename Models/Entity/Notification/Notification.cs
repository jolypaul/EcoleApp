using EcoleApp.Models.Entity.Auth;

namespace EcoleApp.Models.Entity.Notification
{
    public class Notification
    {
        public int Id { get; set; }

        public string UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        public string Message { get; set; } = string.Empty;
        public bool EstLu { get; set; }

        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;
    }
}
