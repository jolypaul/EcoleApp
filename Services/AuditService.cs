using System.Text;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.AdministrationEtAudit;

namespace EcoleApp.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // LOG D'AUDIT
        // ============================
        public async Task LogAsync(string utilisateurId, string action, string details)
        {
            var log = new AuditLog
            {
                UtilisateurId = utilisateurId,
                Action = action,
                Details = details,
                DateAction = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // ============================
        // LISTE + FILTRES + PAGINATION
        // ============================
        public async Task<(List<AuditLog> logs, int total)> GetAuditsAsync(
            string? acteurEmail,
            string? action,
            DateTime? dateDebut,
            DateTime? dateFin,
            int page,
            int pageSize)
        {
            var query = _context.AuditLogs
                .Include(a => a.Utilisateur)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(acteurEmail))
                query = query.Where(a => a.Utilisateur!.Email.Contains(acteurEmail));

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action.Contains(action));

            if (dateDebut.HasValue)
                query = query.Where(a => a.DateAction >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(a => a.DateAction <= dateFin.Value);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.DateAction)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, total);
        }

        // ============================
        // EXPORT CSV
        // ============================
        public async Task<byte[]> ExportCsvAsync(DateTime? debut, DateTime? fin)
        {
            var logs = await _context.AuditLogs
                .Include(a => a.Utilisateur)
                .Where(a =>
                    (!debut.HasValue || a.DateAction >= debut) &&
                    (!fin.HasValue || a.DateAction <= fin))
                .OrderByDescending(a => a.DateAction)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Date;Acteur;Action;Details");

            foreach (var l in logs)
            {
                sb.AppendLine($"{l.DateAction};{l.Utilisateur?.Email};{l.Action};{l.Details}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // ============================
        // EXPORT PDF (UC6)
        // ============================
        public async Task<byte[]> ExportPdfAsync(DateTime? debut, DateTime? fin)
        {
            var logs = await _context.AuditLogs
                .Include(a => a.Utilisateur)
                .Where(a =>
                    (!debut.HasValue || a.DateAction >= debut) &&
                    (!fin.HasValue || a.DateAction <= fin))
                .OrderByDescending(a => a.DateAction)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header()
                        .Text("Journal d'audit")
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Acteur").Bold();
                            header.Cell().Text("Action").Bold();
                            header.Cell().Text("Détails").Bold();
                        });

                        foreach (var log in logs)
                        {
                            table.Cell().Text(log.DateAction.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Text(log.Utilisateur?.Email ?? "N/A");
                            table.Cell().Text(log.Action);
                            table.Cell().Text(log.Details);
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }
    }
}
