using EcoleApp.Models.DAL;
using EcoleApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

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



var app = builder.Build();

//  Configuration du pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.UseAuthorization();

//  Route par défaut MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
