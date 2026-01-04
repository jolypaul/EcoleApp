using System.ComponentModel.DataAnnotations;

namespace EcoleApp.Models.Entity.Auth
{
    public class Session
    {
        [Key]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        public string UtilisateurId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }

        public string? RemoteIp { get; set; }
        public string? UserAgent { get; set; }

        public bool IsActive => EndTime == null;
    }
}
