namespace EcoleApp.Models.Entity.Auth
{
    public class Role
    {
        public int Id { get; set; }
        public string NomRole { get; set; } = string.Empty;
        public string Responsabilite { get; set; } = string.Empty;

        public ICollection<Utilisateur>? Utilisateurs { get; set; }
    }
}
