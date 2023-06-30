//'global using' permet d'incorporer ce mod�le dans tous les fichiers du programme
global using VWA_TFE.Models;
global using VWA_TFE.ViewModel; 

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Ajout des services utilis�s dans le programme
builder.Services.AddControllersWithViews();

//On r�cup�re les informations de connexion enregistr�es dans le fichier de configuration appSettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Ajout du contexte de database, configur� gr�ce au mod�le AppDbContext
builder.Services.AddDbContext<AppDbContext>(db =>
    //On utilise un serveur MSSQL (il faut installer un packet nuGet diff�rent si on a un autre SGBD)
    db.UseSqlServer(connectionString)
);

//On incorpore le service Identity, on lui fournit les informations � propos de l'utilisateur et du r�le
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

//On incorpore les donn�es du mail enregistr�es dans 'appsettings.json' au Mod�le MailSettings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

//Si l'application n'est pas en d�veloppement
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
