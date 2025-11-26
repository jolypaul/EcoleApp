namespace EcoleApp.Models.Entity.Auth
{
    public class Utilisateur
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
