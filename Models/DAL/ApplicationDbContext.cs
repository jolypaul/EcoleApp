using Microsoft.EntityFrameworkCore;

using EcoleApp.Models.Entity.AdministrationEtAudit;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Entity.JustificationDesHeures;
using EcoleApp.Models.Entity.Notification;
using EcoleApp.Models.Entity.RapportEtStatistique;
using EcoleApp.Models.Entity.SeanceDeCours;
using EcoleApp.Utilitaires;

namespace EcoleApp.Models.DAL
{
    public class ApplicationDbContext : DbContext
    {
        // Constructeur
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ======================
        // AUTHENTIFICATION
        // ======================
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Etudiant> Etudiants { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Enseignant> Enseignants { get; set; }

        // ======================
        // COURS & GROUPES
        // ======================
        public DbSet<Cours> Cours { get; set; }
        public DbSet<Groupe> Groupes { get; set; }
        public DbSet<Seance> Seances { get; set; }
        public DbSet<CahierDeTexte> CahiersDeTexte { get; set; }
        public DbSet<Justificatif> Justificatifs { get; set; }

        // ======================
        // NOTIFICATIONS & RAPPORTS
        // ======================
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RapportAssiduite> Rapports { get; set; }

        // ======================
        // AUDIT & PERMISSIONS
        // ======================
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================
            // Seed des rôles
            // ======================
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    NomRole = "Admin",
                    Responsabilite = "Gestion complète du système"
                },
                new Role
                {
                    Id = 2,
                    NomRole = "Enseignant",
                    Responsabilite = "Gestion des séances et présences"
                },
                new Role
                {
                    Id = 3,
                    NomRole = "Etudiant",
                    Responsabilite = "Consultation des présences"
                },
                new Role
                {
                    Id = 4,
                    NomRole = "Delegue",
                    Responsabilite = "Gestion des présences de la classe"
                }
            );

            // ======================
            // Seed du compte Admin
            // ======================
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = "ADMIN-001",
                    NomComplet = "Administrateur Principal",
                    Email = "admin@asp.com",
                    MotDePasse = PasswordHelper.HashPassword("admin@237"),
                    RoleId = 1,
                    Poste = "Administrateur Système"
                }
            );
        }
    }
}
