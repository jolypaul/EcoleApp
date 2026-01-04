using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.SeanceDeCours;
using EcoleApp.Models.Enums;
using EcoleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using EcoleApp.Exceptions;

namespace EcoleApp.Services.Implementations
{
    public class SeanceService : ISeanceService
    {
        private readonly ApplicationDbContext _context;

        public SeanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Seance> CreerSeanceAsync(
            int coursId,
            int groupeId,
            DateTime date,
            string salle,
            string type,
            string enseignantId)
        {
            var cours = await _context.Cours
                .FirstOrDefaultAsync(c => c.Id == coursId && c.EnseignantId == enseignantId);

            if (cours == null)
                throw new BusinessException("Cours invalide ou non autorisé.");

            var seance = new Seance
            {
                CoursId = coursId,
                GroupeId = groupeId,
                Date = date,
                Salle = salle,
                Type = type,
                Etat = EtatSeance.Brouillon
            };

            _context.Seances.Add(seance);
            await _context.SaveChangesAsync();

            return seance;
        }

        public async Task ModifierSeanceAsync(
            int seanceId,
            DateTime date,
            string salle,
            string type,
            string enseignantId)
        {
            var seance = await _context.Seances
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == seanceId);

            if (seance == null)
                throw new BusinessException("Séance introuvable.");

            if (seance.Etat == EtatSeance.Validee)
                throw new BusinessException("Séance validée, modification interdite.");

            if (seance.Cours?.EnseignantId != enseignantId)
                throw new BusinessException("Accès non autorisé.");

            seance.Date = date;
            seance.Salle = salle;
            seance.Type = type;

            await _context.SaveChangesAsync();
        }

        public async Task ValiderSeanceAsync(int seanceId, string enseignantId)
        {
            var seance = await _context.Seances
                .Include(s => s.Cours)
                .Include(s => s.Appel)
                .FirstOrDefaultAsync(s => s.Id == seanceId);

            if (seance == null)
                throw new BusinessException("Séance introuvable.");

            if (seance.Cours?.EnseignantId != enseignantId)
                throw new BusinessException("Accès non autorisé.");

            if (seance.Etat == EtatSeance.Validee)
                throw new BusinessException("Séance déjà validée.");

            seance.Etat = EtatSeance.Validee;

            if (seance.Appel != null)
                seance.Appel.EstVerrouille = true;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Seance>> ConsulterSeancesAsync(string utilisateurId)
        {
            return await _context.Seances
                .Include(s => s.Cours)
                .Include(s => s.Groupe)
                .Where(s =>
                    s.Cours!.EnseignantId == utilisateurId ||
                    s.Groupe!.Etudiants!.Any(e => e.Id == utilisateurId))
                .ToListAsync();
        }
    }
}
