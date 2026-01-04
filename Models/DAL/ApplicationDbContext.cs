using EcoleApp.Models.Entity.AdministrationEtAudit;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Models.Entity.JustificationDesHeures;
using EcoleApp.Models.Entity.Notification;
using EcoleApp.Models.Entity.RapportEtStatistique;
using EcoleApp.Models.Entity.SeanceDeCours;
using EcoleApp.Utilitaires;
using Microsoft.EntityFrameworkCore;

namespace EcoleApp.Models.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ======================
        // AUTHENTIFICATION
        // ======================
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Enseignant> Enseignants { get; set; }
        public DbSet<Etudiant> Etudiants { get; set; }
        public DbSet<Session> Sessions { get; set; }

        // ======================
        // COURS / GROUPES / SÉANCES
        // ======================
        public DbSet<Cours> Cours { get; set; }
        public DbSet<Groupe> Groupes { get; set; }
        public DbSet<Seance> Seances { get; set; }

        // ======================
        // PRISE D’APPEL
        // ======================
        public DbSet<Appel> Appels { get; set; }
        public DbSet<LigneAppel> LignesAppel { get; set; }

        // ======================
        // CAHIER DE TEXTE & JUSTIFICATIFS
        // ======================
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
            // SEED DES RÔLES
            // ======================
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = "1",
                    NomRole = "Admin",
                    Responsabilite = "Gestion complète du système"
                },
                new Role
                {
                    Id = "2",
                    NomRole = "Enseignant",
                    Responsabilite = "Gestion des séances et présences"
                },
                new Role
                {
                    Id = "3",
                    NomRole = "Etudiant",
                    Responsabilite = "Consultation des présences"
                },
                new Role
                {
                    Id = "4",
                    NomRole = "Delegue",
                    Responsabilite = "Gestion des présences de la classe"
                }
            );

            // ======================
            // SEED ADMIN PAR DÉFAUT
            // ======================
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = "ADMIN-001",
                    NomComplet = "Administrateur Principal",
                    Email = "admin@237.com",
                    MotDePasseHash = PasswordHelper.HashPassword("admin@237"),
                    RoleId = "1",
                    Poste = "Administrateur Système"
                }
            );

            modelBuilder.Entity<Groupe>()
            .HasMany(g => g.Etudiants)
            .WithOne(e => e.Groupe)
            .HasForeignKey(e => e.GroupeId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Utilisateur>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Utilisateurs)
            .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.Utilisateur)
            .WithMany()
            .HasForeignKey(a => a.UtilisateurId);

            modelBuilder.Entity<Session>()
                .HasKey(s => s.SessionId);

            modelBuilder.Entity<Session>()
                .HasOne<EcoleApp.Models.Entity.Auth.Utilisateur>()
                .WithMany()
                .HasForeignKey(s => s.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
