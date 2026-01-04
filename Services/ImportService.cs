using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Entity.SeanceDeCours;
using EcoleApp.Utilitaires;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;

namespace EcoleApp.Services
{
    public class ImportService
    {
        private readonly ApplicationDbContext _context;

        public ImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(int created, int skipped, List<string> errors)> ImportStudentsFromExcelAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var errors = new List<string>();
            int created = 0, skipped = 0;

            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();
            if (ws == null)
            {
                errors.Add("Le fichier Excel ne contient pas de feuille.");
                return (created, skipped, errors);
            }

            // Read header
            var header = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int col = 1;
            while (!string.IsNullOrWhiteSpace(ws.Cells[1, col].Text))
            {
                header[ws.Cells[1, col].Text.Trim()] = col;
                col++;
            }

            // Expected fields: Matricule, NomComplet, Email, Filiere, Niveau, Groupe
            int row = 2;
            while (row <= ws.Dimension.End.Row)
            {
                // stop when empty row (all cells empty)
                bool empty = true;
                for (int c = 1; c <= col; c++)
                {
                    if (!string.IsNullOrWhiteSpace(ws.Cells[row, c].Text)) { empty = false; break; }
                }
                if (empty) break;

                string matricule = GetCell(ws, header, "Matricule", 1, row);
                string nom = GetCell(ws, header, "NomComplet", 2, row);
                string email = GetCell(ws, header, "Email", 3, row);
                string filiere = GetCell(ws, header, "Filiere", 4, row);
                string niveau = GetCell(ws, header, "Niveau", 5, row);
                string groupeName = GetCell(ws, header, "Groupe", 6, row);

                if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(matricule))
                {
                    errors.Add($"Ligne {row} ignorée : email et matricule vides.");
                    skipped++;
                    row++;
                    continue;
                }

                // compute firstName from NomComplet
                string firstName = "";
                if (!string.IsNullOrWhiteSpace(nom))
                {
                    var parts = nom.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0) firstName = parts[0];
                }

                // default email/login if missing
                if (string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(firstName))
                {
                    email = firstName.ToLowerInvariant() + "@237.com";
                }

                // check existing by email or matricule
                var existsByEmail = await _context.Utilisateurs.AnyAsync(u => u.Email == email);
                var existsByMatricule = await _context.Utilisateurs.OfType<Etudiant>().AnyAsync(e => e.Matricule == matricule);
                if (existsByEmail || existsByMatricule)
                {
                    skipped++;
                    row++;
                    continue;
                }

                // find or create group
                Groupe? groupe = null;
                if (!string.IsNullOrWhiteSpace(groupeName))
                {
                    groupe = await _context.Groupes.FirstOrDefaultAsync(g => g.Nom == groupeName.Trim());
                    if (groupe == null)
                    {
                        groupe = new Groupe { Nom = groupeName.Trim() };
                        _context.Groupes.Add(groupe);
                        await _context.SaveChangesAsync();
                    }
                }

                // initial password: firstName (lowercase) if available else 'changeme123'
                string initialPassword = !string.IsNullOrWhiteSpace(firstName) ? firstName.ToLowerInvariant() : "changeme123";

                var etudiant = new Etudiant
                {
                    Id = Guid.NewGuid().ToString(),
                    NomComplet = nom ?? string.Empty,
                    Email = email ?? string.Empty,
                    MotDePasseHash = PasswordHelper.HashPassword(initialPassword),
                    Matricule = matricule ?? string.Empty,
                    Filiere = filiere ?? string.Empty,
                    Niveau = niveau ?? string.Empty,
                    GroupeId = groupe?.Id,
                    MustChangePassword = true
                };

                // Assign role '3' (Etudiant)
                etudiant.RoleId = "3";

                _context.Utilisateurs.Add(etudiant);
                await _context.SaveChangesAsync();
                created++;

                row++;
            }

            return (created, skipped, errors);
        }

        private string GetCell(ExcelWorksheet ws, Dictionary<string,int> header, string key, int fallbackCol, int row)
        {
            if (header.TryGetValue(key, out var c))
                return ws.Cells[row, c].Text.Trim();
            return ws.Cells[row, fallbackCol].Text.Trim();
        }
    }
}
