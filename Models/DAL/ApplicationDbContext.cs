using EcoleApp.Models.Entity.AdministrationEtAudit;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Entity.JustificationDesHeures;
using EcoleApp.Models.Entity.Notification;
using EcoleApp.Models.Entity.RapportEtStatistique;
using EcoleApp.Models.Entity.SeanceDeCours;
using Microsoft.EntityFrameworkCore;

namespace EcoleApp.Models.DAL
{
    public class ApplicationDbContext : DbContext
    {
        //  Constructeur propre et typé
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //  AUTH
        public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Etudiant> Etudiants => Set<Etudiant>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Enseignant> Enseignants => Set<Enseignant>();

        //  COURS & GROUPES
        public DbSet<Cours> Cours => Set<Cours>();
        public DbSet<Groupe> Groupes => Set<Groupe>();
        public DbSet<Seance> Seances => Set<Seance>();
        public DbSet<CahierDeTexte> CahiersDeTexte => Set<CahierDeTexte>();
        public DbSet<Justificatif> Justificatifs => Set<Justificatif>();

        //  NOTIFICATIONS & RAPPORTS
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<RapportAssiduite> Rapports => Set<RapportAssiduite>();

        // AUDIT & PERMISSIONS
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Permission> Permissions => Set<Permission>();
    }
}
