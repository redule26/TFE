using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using VWA_TFE.Models;
using VWA_TFE.ViewModel;

namespace VWA_TFE.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MailSettings _mailSettings;
        private readonly UserManager<AppUser> _userManager;

        //injection des dépendances dans le constructeur de AccountController          
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /*
         _signInManager = signInManager;
            _mailSettings = mailSettings.Value;
            _roleManager = roleManager;*/

        //Get : Account/Login
        [HttpGet]
        [AllowAnonymous] //Attribut qui permet à un utilisateur d'accéder à cette méthode sans être connecté 
        public IActionResult Login(string? returnUrl = null)
        {
            LoginViewModel loginViewModel = new LoginViewModel();
            //Si la tentative de connexion est réussie ou a échoué
            loginViewModel.ReturnUrl = returnUrl ?? Url.Content("~/"); 
            return View(loginViewModel);
        }

        [HttpPost]
        [AllowAnonymous] //Attribut qui permet à un utilisateur d'accéder à cette méthode sans être connecté 
        [ValidateAntiForgeryToken] //permet d'éviter le Cross Site Request Forgery
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string? returnUrl)
        {
            //Si les attributs de contrôle du modèle sont validés
            if (ModelState.IsValid)
            {
                //On enregistre le résultat de la connexion de l'utilisateur
                var result = await _signInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded) //On redirige l'utilisateur en cas de succès
                {
                    return RedirectToAction("Index", "Home");
                }
                else //En cas d'échec
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(loginViewModel);
                }
                
            }
            //On renvoie une page View() avec comme arguments l'erreur en question du modèle
            return View(loginViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //permet d'éviter le Cross Site Request Forgery
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); //déconnexion de l'utilisateur
            return RedirectToAction("Index", "Home"); //redirection vers la page d'acceuil
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            //Le ViewModel va faire la vérification du modèle pour que je ne doive pas la faire ici
            RegisterViewModel registerViewModel = new RegisterViewModel();
            registerViewModel.ReturnUrl = returnUrl;

            return View(registerViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl = null)
        {
            registerViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/"); //ternary operator

            if (_userManager.Users.FirstOrDefault(u => u.UserName == registerViewModel.UserName) != null) //Si le nom d'utilisateur est déjà utilisé
            {
                ModelState.AddModelError("Username", "This username is already in use.");
            }
            else if (_userManager.Users.FirstOrDefault(u => u.Email == registerViewModel.Email) != null) //Si le mail est déjà utilisé
            {
                ModelState.AddModelError("Email", "This email is already in use.");
            }
            else if (ModelState.IsValid) //si le modèle est validé
            {
                //Création d'un nouveau AppUser
                var user = new AppUser { Email = registerViewModel.Email, UserName = registerViewModel.UserName, FirstName = registerViewModel.FirstName, LastName = registerViewModel.LastName };
                //on stocke le résultat de l'ajout de l'utilisateur à la database
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                //Si l'utilisateur est crée
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    //Création du mail
                    var email = new MimeMessage();
                    email.From.Add(MailboxAddress.Parse(_mailSettings.Mail));
                    email.To.Add(MailboxAddress.Parse(registerViewModel.Email)); 
                    email.Subject = $"{user.FirstName} {user.LastName}, your VWA account has been created.";

                    //Creation du contenu
                    var builder = new BodyBuilder();
                    builder.TextBody = $"Your login : {user.UserName} \n your password : {registerViewModel.Password}";
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
                    return LocalRedirect(returnUrl);
                }
                //Si le modèle n'a pas été validé
                ModelState.AddModelError("All", "User could not be created. Password not unique enough");
            }
            return View(registerViewModel);
        }

        //GET : Account/Profile
        [HttpGet]
        [Authorize] //permet de donner l'accès seulement à un utilisateur connecté
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }
    }
}
