using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Utilitaires;
using Microsoft.EntityFrameworkCore;

namespace EcoleApp.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Utilisateur?> VerifierIdentiteAsync(string email, string motDePasse)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (utilisateur == null)
                return null;

            bool motDePasseValide =
                PasswordHelper.VerifyPassword(motDePasse, utilisateur.MotDePasseHash);

            return motDePasseValide ? utilisateur : null;
        }
    }
}
