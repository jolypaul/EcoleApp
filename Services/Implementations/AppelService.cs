using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Models.Enums;
using EcoleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcoleApp.Services.Implementations
{
    public class AppelService : IAppelService
    {
        private readonly ApplicationDbContext _context;

        public AppelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Appel> CreerAppelAsync(int seanceId, string delegueId)
        {
            var seance = await _context.Seances
                .Include(s => s.Groupe)
                .ThenInclude(g => g.Etudiants)
                .FirstOrDefaultAsync(s => s.Id == seanceId);

            if (seance == null)
                throw new BusinessException("Séance introuvable.");

            if (seance.Etat == EtatSeance.Validee)
                throw new BusinessException("Séance validée, appel interdit.");

            if (seance.Appel != null)
                throw new BusinessException("Appel déjà existant.");

            var appel = new Appel
            {
                SeanceId = seanceId,
                DelegueId = delegueId
            };

            foreach (var etudiant in seance.Groupe!.Etudiants!)
            {
                appel.LignesAppel.Add(new LigneAppel
                {
                    EtudiantId = etudiant.Id,
                    Statut = StatutPresence.Present
                });
            }

            _context.Appels.Add(appel);
            await _context.SaveChangesAsync();

            return appel;
        }

        public async Task SaisirAppelAsync(int appelId, Dictionary<string, StatutPresence> presences)
        {
            var appel = await _context.Appels
                .Include(a => a.Seance)
                .Include(a => a.LignesAppel)
                .FirstOrDefaultAsync(a => a.Id == appelId);

            if (appel == null)
                throw new BusinessException("Appel introuvable.");

            if (appel.EstVerrouille || appel.Seance!.Etat == EtatSeance.Validee)
                throw new BusinessException("Appel verrouillé.");

            foreach (var ligne in appel.LignesAppel)
            {
                if (presences.TryGetValue(ligne.EtudiantId, out var statut))
                {
                    ligne.Statut = statut;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LigneAppel>> ConsulterAbsencesAsync(DateTime debut, DateTime fin)
        {
            return await _context.LignesAppel
                .Include(l => l.Etudiant)
                .Include(l => l.Appel)
                .ThenInclude(a => a.Seance)
                .Where(l =>
                    l.Statut != StatutPresence.Present &&
                    l.Appel.Seance.Date >= debut &&
                    l.Appel.Seance.Date <= fin)
                .ToListAsync();
        }
    }
}
