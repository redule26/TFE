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

        //private readonly RoleManager<IdentityRole> _roleManager;
        /*
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MailSettings _mailSettings;*/


        public AdministrationController(UserManager<AppUser> userManager, IOptions<MailSettings> mailSettings/*, SignInManager<AppUser> signInManager, IOptions<MailSettings> mailSettings, RoleManager<IdentityRole> roleManager, AppDbContext dbContext*/)
        {
            _userManager = userManager;
            _mailSettings = mailSettings.Value;
            //_roleManager = roleManager;
            /* _signInManager = signInManager;
             _mailSettings = mailSettings.Value;
             _roleManager = roleManager;
             _dbContext = dbContext;*/
        }

        //GET 
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            EditViewModel editViewModel = new EditViewModel();
            editViewModel.UserEmail = userEmail;
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

        public async Task<IActionResult> Delete(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if(user.Id != "288df3c7-aaab-467c-b8bb-09184a135d79")
            {
                await _userManager.DeleteAsync(user);
            } 
            return RedirectToAction("Index", "Administration");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser { Email = model.Email, UserName = model.UserName, FirstName = model.FirstName, LastName = model.LastName };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    var email = new MimeMessage();
                    email.From.Add(MailboxAddress.Parse(_mailSettings.Mail));
                    email.To.Add(MailboxAddress.Parse(model.Email));
                    email.Subject = $"{user.FirstName} {user.LastName}, your VWA account has been created.";

                    var builder = new BodyBuilder();
                    builder.TextBody = $"Login : {user.UserName} \nPassword : {model.Password}";
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


                    return RedirectToAction("Index", "Administration");
                }

                ModelState.AddModelError("Password", "User could not be created. Password not unique enough");
            }
            return View(model);
        }

        public string generatePassword()
        {
            return "";
        }
    }
}
