using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Utilitaires;
using Microsoft.EntityFrameworkCore;

namespace EcoleApp.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public AdminService(
            ApplicationDbContext context,
            AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task CreerUtilisateurAsync(
            string nom,
            string email,
            string motDePasse,
            int roleId,
            string adminId)
        {
            if (await _context.Utilisateurs.AnyAsync(u => u.Email == email))
                throw new Exception("Email déjà utilisé.");

            var utilisateur = new Utilisateur
            {
                Id = Guid.NewGuid().ToString(),
                NomComplet = nom,
                Email = email,
                MotDePasseHash = PasswordHelper.HashPassword(motDePasse),
                RoleId = roleId
            };

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                adminId,
                "Création utilisateur",
                $"Création du compte {email}"
            );
        }
        public async Task ChangerRoleAsync(
    string utilisateurId,
    int nouveauRoleId,
    string adminId)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == utilisateurId);

            if (utilisateur == null)
                throw new Exception("Utilisateur introuvable.");

            var ancienRole = utilisateur.Role?.NomRole ?? "Aucun";

            utilisateur.RoleId = nouveauRoleId;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                adminId,
                "Modification rôle",
                $"Utilisateur {utilisateur.Email} : {ancienRole} → RoleId {nouveauRoleId}"
            );
        }



    }
}
