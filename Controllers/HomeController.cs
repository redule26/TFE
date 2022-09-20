using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VWA_TFE.Models;

namespace VWA_TFE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        //my custom route : https://localhost:7092/Home/Add/u=username&p=password&uid=uid
        [Route("Home/Add/u={username}&p={password}&uid={uid}")]
        public IActionResult WriteToDB(string username, string password, string uid = "??")
        {
            _context.Users.AddAsync(new User { Username = username, Password = password, Uid = uid });
            _context.SaveChanges();
            
            //it's useless to catch the exception, it already get caught by the service inside Program.cs (line 13 -> 17)
            return Content("success");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}