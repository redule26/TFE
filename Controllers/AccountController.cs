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
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MailSettings _mailSettings;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IOptions<MailSettings> mailSettings, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailSettings = mailSettings.Value;
            _roleManager = roleManager;
        }

        /*public IActionResult Index()
        {
            return View();
        }*/

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            LoginViewModel loginViewModel = new LoginViewModel();
            loginViewModel.ReturnUrl = returnUrl ?? Url.Content("~/");
            return View(loginViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] //for security
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                //, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false
                var result = await _signInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(loginViewModel);
                }
                
            }
            return View(loginViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //Get 
        [AllowAnonymous]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            //The viewModel is going to do the validation (so i don't need to do it inside de controller)
            RegisterViewModel registerViewModel = new RegisterViewModel();
            registerViewModel.ReturnUrl = returnUrl;
            return View(registerViewModel);
        }

        //Post data from the form
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl = null)
        {
            //if(!await _roleManager.RoleExistsAsync("Administrator"))
            //{
            //    await _roleManager.CreateAsync(new IdentityRole("Administrator"));
            //}

            registerViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/"); //ternary operator

            //Instead of checking one by one every fields of the form we are just going to check with the model's attributes
            if (ModelState.IsValid)
            {
                var user = new AppUser { Email = registerViewModel.Email, UserName = registerViewModel.UserName, FirstName = registerViewModel.FirstName, LastName = registerViewModel.LastName };
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    var email = new MimeMessage();
                    email.From.Add(MailboxAddress.Parse(_mailSettings.Mail));
                    email.To.Add(MailboxAddress.Parse(registerViewModel.Email)); 
                    email.Subject = $"{user.FirstName} {user.LastName}, your VWA account has been created.";

                    var builder = new BodyBuilder();
                    builder.TextBody = $"Your login : {user.UserName} \n your password : {registerViewModel.Password}";
                    email.Body = builder.ToMessageBody();

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
                        throw ex;
                    }


                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError("Password", "User could not be created. Password not unique enough");
            }
            return View(registerViewModel);
        }

        //Get profile view (only if a user is logged in)
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }
    }
}
