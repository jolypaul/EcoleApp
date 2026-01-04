using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Models.Enums;
using EcoleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ExportAbsenceService : IExportAbsenceService
{
    private readonly ApplicationDbContext _context;

    public ExportAbsenceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportExcelAsync(DateTime debut, DateTime fin)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var lignes = await ChargerAbsences(debut, fin);

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Absences");

        ws.Cells[1, 1].Value = "Étudiant";
        ws.Cells[1, 2].Value = "Groupe";
        ws.Cells[1, 3].Value = "Cours";
        ws.Cells[1, 4].Value = "Date";
        ws.Cells[1, 5].Value = "Statut";

        int row = 2;
        foreach (var l in lignes)
        {
            ws.Cells[row, 1].Value = l.Etudiant.NomComplet;
            ws.Cells[row, 2].Value = l.Appel.Seance.Groupe.Nom;
            ws.Cells[row, 3].Value = l.Appel.Seance.Cours.Nom;
            ws.Cells[row, 4].Value = l.Appel.Seance.Date.ToString("dd/MM/yyyy");
            ws.Cells[row, 5].Value = l.Statut.ToString();
            row++;
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }



    public async Task<byte[]> ExportPdfAsync(DateTime debut, DateTime fin)
    {
        var lignes = await ChargerAbsences(debut, fin);

        var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Rapport d'absences")
                    .FontSize(18).Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Étudiant").Bold();
                            header.Cell().Text("Groupe").Bold();
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Statut").Bold();
                        });

                        foreach (var l in lignes)
                        {
                            table.Cell().Text(l.Etudiant.NomComplet);
                            table.Cell().Text(l.Appel.Seance.Groupe.Nom);
                            table.Cell().Text(l.Appel.Seance.Date.ToString("dd/MM/yyyy"));
                            table.Cell().Text(l.Statut.ToString());
                        }
                    });
                });
            });

        return document.GeneratePdf();
    }

    private async Task<List<LigneAppel>> ChargerAbsences(DateTime debut, DateTime fin)
    {
        return await _context.LignesAppel
            .Include(l => l.Etudiant)
            .Include(l => l.Appel)
                .ThenInclude(a => a.Seance)
                    .ThenInclude(s => s.Cours)
            .Include(l => l.Appel.Seance.Groupe)
            .Where(l =>
                l.Statut != StatutPresence.Present &&
                l.Appel.Seance.Date >= debut &&
                l.Appel.Seance.Date <= fin)
            .ToListAsync();
    }

}
