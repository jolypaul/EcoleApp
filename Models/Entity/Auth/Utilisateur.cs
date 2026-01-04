namespace EcoleApp.Models.Entity.Auth
{
    public class Utilisateur
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasseHash { get; set; } = string.Empty;

        public string RoleId { get; set; } = string.Empty;
        public Role? Role { get; set; }

        // Force user to change initial password on first login
        public bool MustChangePassword { get; set; } = false;
    }
}
