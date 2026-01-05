using EcoleApp.Models.DAL;
using EcoleApp.Security.Requirements;
using EcoleApp.Services;
using EcoleApp.Services.Implementations;
using EcoleApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using MySqlConnector;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Utilitaires;
using EcoleApp.Middlewares;


var builder = WebApplication.CreateBuilder(args);

//  Ajout des contrôleurs + vues
builder.Services.AddControllersWithViews();

//  Récupération de la chaîne de connexion MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//  Configuration UNIQUE de MySQL avec EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ImportService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<ISeanceService, SeanceService>();
builder.Services.AddScoped<IAppelService, AppelService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SeanceEditable", policy =>
        policy.Requirements.Add(new SeanceEditableRequirement()));

    options.AddPolicy("AppelEditable", policy =>
        policy.Requirements.Add(new AppelEditableRequirement()));

    options.AddPolicy("AbsenceConsultation", policy =>
        policy.Requirements.Add(new AbsenceConsultationRequirement()));
});
builder.Services.AddScoped<IAuthorizationHandler, SeanceEditableHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AppelEditableHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AbsenceConsultationHandler>();
builder.Services.AddScoped<IExportAbsenceService, ExportAbsenceService>();



var app = builder.Build();

// Ensure database exists and migrations are applied at startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        // Try to ensure the database itself exists (server level create DB if missing)
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var csb = new MySqlConnectionStringBuilder(connectionString);
            var dbName = csb.Database;

            if (!string.IsNullOrWhiteSpace(dbName))
            {
                // Remove database to open a server-level connection
                csb.Database = string.Empty;
                using var serverConn = new MySqlConnection(csb.ConnectionString);
                serverConn.Open();
                using var cmd = serverConn.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
                cmd.ExecuteNonQuery();
                serverConn.Close();
            }
        }

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Attempt to apply migrations first
        try
        {
            db.Database.Migrate();
            Console.WriteLine("Database migrated successfully.");
        }
        catch (Exception migrateEx)
        {
            Console.WriteLine("Migration failed: " + migrateEx.Message);

            // If migrations failed, ensure DB created from model and seed essential data
            try
            {
                // Check if main table exists
                var csb = new MySqlConnectionStringBuilder(connectionString);
                var dbName = csb.Database;
                bool utilisateursExists = false;

                try
                {
                    using var conn = db.Database.GetDbConnection();
                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();

                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schema AND LOWER(TABLE_NAME) = 'utilisateurs'";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@schema";
                    p.Value = dbName;
                    cmd.Parameters.Add(p);

                    var res = cmd.ExecuteScalar();
                    if (res != null && Convert.ToInt32(res) > 0)
                        utilisateursExists = true;
                }
                catch (Exception checkEx)
                {
                    Console.WriteLine("Table existence check failed: " + checkEx.Message);
                }

                if (!utilisateursExists)
                {
                    var created = db.Database.EnsureCreated();
                    Console.WriteLine($"EnsureCreated executed. Created: {created}");

                    // Seed roles and default admin if missing
                    try
                    {
                        if (!db.Roles.Any())
                        {
                            db.Roles.AddRange(
                                new Role { Id = "1", NomRole = "Admin", Responsabilite = "Gestion complète du système" },
                                new Role { Id = "2", NomRole = "Enseignant", Responsabilite = "Gestion des séances et présences" },
                                new Role { Id = "3", NomRole = "Etudiant", Responsabilite = "Consultation des présences" },
                                new Role { Id = "4", NomRole = "Delegue", Responsabilite = "Gestion des présences de la classe" }
                            );
                            db.SaveChanges();
                            Console.WriteLine("Seeded roles.");
                        }

                        if (!db.Utilisateurs.Any(u => u.Id == "ADMIN-001"))
                        {
                            var admin = new Admin
                            {
                                Id = "ADMIN-001",
                                NomComplet = "Administrateur Principal",
                                Email = "admin@237.com",
                                MotDePasseHash = PasswordHelper.HashPassword("admin@237"),
                                RoleId = "1",
                                Poste = "Administrateur Système"
                            };

                            db.Utilisateurs.Add(admin);
                            db.SaveChanges();
                            Console.WriteLine("Seeded default admin.");
                        }
                    }
                    catch (Exception seedEx)
                    {
                        Console.WriteLine("Seeding failed: " + seedEx.Message);
                    }
                }
            }
            catch (Exception ensureEx)
            {
                Console.WriteLine("EnsureCreated also failed: " + ensureEx.Message);
                throw; // rethrow to be visible in logs
            }
        }
    }
    catch (Exception ex)
    {
        // If migration fails keep the app running but log to console with details
        Console.WriteLine("Failed to prepare database: " + ex.Message);
    }
}

//  Configuration du pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// session validation middleware (must be after auth)
app.UseMiddleware<SessionValidationMiddleware>();

//  Route par défaut MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
