using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VWA_TFE.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context)
        {
            _context = context;
        }


        // GET: AuthController
        public ActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string username, string password)
        {
            var userExists = _context.Users.Where(u => u.Username == username && u.Password == password).FirstOrDefault();

            if (userExists == null)
            {
                return Content("User not found :/");
            }

            //create json token and save it into the cookies
            return Content("Success");
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Content("All fields need to be filled");
            }

            if (!_context.Users.Any(u => u.Username == username))
            {
                _context.Users.Add(new User { Username = username, Password = password, Uid = $"{DateTime.Now.ToString("yyyyddhhmmss")}{username}"});
                _context.SaveChanges();
                return Content($"Welcome {username}");
            }
            
            return Content($"User already exists");
        }
    }
}
