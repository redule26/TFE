//'global using' permet d'incorporer ce modèle dans tous les fichiers du programme
global using VWA_TFE.Models;
global using VWA_TFE.ViewModel; 

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Ajout des services utilisés dans le programme
builder.Services.AddControllersWithViews();

//On récupère les informations de connexion enregistrées dans le fichier de configuration appSettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnexion");

//Ajout du contexte de database, configuré grâce au modèle AppDbContext
builder.Services.AddDbContext<AppDbContext>(db =>
    //On utilise un serveur MSSQL (il faut installer un packet nuGet différent si on a un autre SGBD)
    db.UseSqlServer(connectionString)
);

//On incorpore le service Identity, on lui fournit les informations à propos de l'utilisateur et du rôle
builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//Configuration du framework Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
});

//On incorpore les données du mail enregistrées dans 'appsettings.json' au Modèle MailSettings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

//Si l'application n'est pas en développement
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
