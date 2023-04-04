using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VWA_TFE.Models;

namespace VWA_TFE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Models.AppDbContext _context;

        //dependency injection 
        public HomeController(ILogger<HomeController> logger, Models.AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //GET : On récupère la page /Home/Index ou /Home 
        public IActionResult Index()
        {
            ViewBag.Test = generatePassword();
            return View();
        }

        //GET : /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        //GET : /Home/About
        public IActionResult About()
        {
            return View();
        }

        //GET : /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }


        //GET : /Home/Error?id=... 
        //Renvoie une vue 'Error' à l'aide d'un ID pour signaler à l'utilisateur qu'il y a une erreur
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] 
        //Attribut qui dit que la réponse envoyée ne doit pas être sauvegardée en cache chez le client ou le serveur
        public IActionResult Error()
        {
            //Création d'une nouvelle instance de la classe 'ErrorViewModel'
            return View(new ErrorViewModel { 
                //RequestId c'est l'id de la requete actuelle, 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }

        private string generatePassword()
        {
            Random random = new Random();
            string passwordCharacters =
                "_$*@%&!-"
                + "abcdefghijklmnopqrstuvwxyz"
                + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                + "0123456789";

            string password = "";

            for (int i = 0; i < 16; i++)
            {
                password.Append(passwordCharacters[random.Next(passwordCharacters.Length)]);
            };

            return password;
        }
    }
}