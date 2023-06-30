using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using MimeKit;
using VWA_TFE.Models;
using VWA_TFE.ViewModel;

namespace VWA_TFE.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly MailSettings _mailSettings;

        public AdministrationController(UserManager<AppUser> userManager, IOptions<MailSettings> mailSettings)
        {
            _userManager = userManager;
            _mailSettings = mailSettings.Value;
            //_roleManager = roleManager;
            /* _signInManager = signInManager;
             _mailSettings = mailSettings.Value;
             _roleManager = roleManager;
             _dbContext = dbContext;*/
        }

        //GET : Administration/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        //GET : Administration/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id); //On cherche l'utilisateur dont le mail est userEmail
            EditViewModel editViewModel = new EditViewModel();
            editViewModel.UserEmail = user.Email;
            editViewModel.UserName = user.UserName;
            editViewModel.FirstName = user.FirstName;
            editViewModel.LastName = user.LastName;
            editViewModel.JobPosition = user.Position;
            editViewModel.Salary = user.Salary;

            return View(editViewModel); 
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.UserEmail);
                user.UserName = model.UserName;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Salary = model.Salary;
                user.Position = model.JobPosition;

                await _userManager.UpdateAsync(user);

                return RedirectToAction("Index", "Administration");
            }
            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            //je vérifie manuellement si le seul administrateur est supprimé
            var user = await _userManager.FindByIdAsync(id);
            if(user.Id != "288df3c7-aaab-467c-b8bb-09184a135d79")
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index", "Administration");
        }


        //Get : Administration/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            //Si tous les attributs du RegisterViewModel sont vérifiés 
            if (ModelState.IsValid)
            {  
                //Création d'un nouveau AppUser
                var user = new AppUser { Email = model.Email, UserName = model.UserName, FirstName = model.FirstName, LastName = model.LastName };
                //on stocke le résultat de l'ajout de l'utilisateur à la database
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    //Création du mail
                    var email = new MimeMessage();
                    email.From.Add(MailboxAddress.Parse(_mailSettings.Mail)); //mail de l'expéditeur
                    email.To.Add(MailboxAddress.Parse(model.Email)); //mail du destinataire
                    email.Subject = $"{user.FirstName} {user.LastName}, your VWA account has been created.";

                    //Creation du contenu
                    var builder = new BodyBuilder();
                    builder.TextBody = $"Login : {user.UserName} \nPassword : {model.Password}";
                    email.Body = builder.ToMessageBody();

                    //On essaye d'envoyer le mail
                    try
                    {
                        using var smtp = new SmtpClient();
                        smtp.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.None);
                        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp.SendAsync(email);
                        smtp.Disconnect(true);
                    }
                    catch (Exception ex)
                    {
                        //Si une erreur apparait, on envoie une exception
                        throw new Exception($"Mail couldn't be sent. {ex.Message}");
                    }

                    //on renvoie vers la page Administration/Index
                    return RedirectToAction("Index", "Administration");
                }

                //Si le modèle n'a pas été validé
                ModelState.AddModelError("Error", "Something must have been wrong, try again... ps: Check if the password is at least 8 characters long and that the mail isn't used");
            }
            return View(model);
        }


        //private string generatePassword()
        //{
        //    Random random = new Random();
        //    string passwordCharacters =
        //        "_$*@%&!-"
        //        + "abcdefghijklmnopqrstuvwxyz"
        //        + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        //        + "0123456789";

        //    string password = "";

        //    for (int i = 0; i < 16; i++)
        //    {
        //        password.Append(passwordCharacters[random.Next(passwordCharacters.Length)]);
        //    };

        //    return password;
        //}
    }
}
