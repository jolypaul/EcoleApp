using EcoleApp.Exceptions;
using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Models.Enums;
using EcoleApp.Services.Interfaces;
using EcoleApp.ViewModels.Appel;
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

        public async Task<int> CreerAppelAsync(int seanceId, string delegueId)
        {
            var seance = await _context.Seances.FindAsync(seanceId)
                ?? throw new BusinessException("Séance introuvable.");

            if (seance.EstValidee)
                throw new BusinessException("Séance déjà validée.");

            var appel = new Appel
            {
                SeanceId = seanceId,
                DelegueId = delegueId,
                DateSaisie = DateTime.UtcNow,
                EstVerrouille = false
            };

            _context.Appels.Add(appel);
            await _context.SaveChangesAsync();

            return appel.Id;
        }

        public async Task<SaisieAppelViewModel> GetSaisieAppelAsync(int appelId)
        {
            var appel = await _context.Appels
                .Include(a => a.Seance)
                    .ThenInclude(s => s.Groupe)
                        .ThenInclude(g => g.Etudiants)
                .Include(a => a.LignesAppel)
                .FirstOrDefaultAsync(a => a.Id == appelId)
                ?? throw new BusinessException("Appel introuvable.");

            if (appel.EstVerrouille)
                throw new BusinessException("Appel déjà validé.");

            var presences = new Dictionary<string, StatutPresence>();
            var names = new Dictionary<string, string>();

            foreach (var e in appel.Seance.Groupe.Etudiants)
            {
                presences[e.Id] = StatutPresence.Present;
                names[e.Id] = e.NomComplet;
            }

            return new SaisieAppelViewModel
            {
                AppelId = appel.Id,
                Presences = presences,
                StudentNames = names
            };
        }

        public async Task EnregistrerSaisieAsync(
            int appelId,
            Dictionary<string, StatutPresence> presences)
        {
            var appel = await _context.Appels
                .Include(a => a.LignesAppel)
                .FirstOrDefaultAsync(a => a.Id == appelId)
                ?? throw new BusinessException("Appel introuvable.");

            if (appel.EstVerrouille)
                throw new BusinessException("Appel verrouillé.");

            appel.LignesAppel.Clear();

            foreach (var p in presences)
            {
                appel.LignesAppel.Add(new LigneAppel
                {
                    EtudiantId = p.Key,      // string ✔
                    Statut = p.Value
                });
            }

            appel.EstVerrouille = true;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LigneAppel>> ConsulterAbsencesAsync(
            DateTime debut,
            DateTime fin)
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
