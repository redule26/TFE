using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VWA_TFE.Models;

namespace VWA_TFE.Controllers
{
    //[Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            if(_context.Users.ToList() != null)
                return View(await _context.Users.ToListAsync());
            else
                return Problem("No users");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //attribute that prevents cross-site request forgery attacks
        public async Task<IActionResult> Create([Bind("Username, Password")] User user) //bind attribute: get name:Username, and Password from input names
        {
            if (User != null)
            {
                //if (_context.Users.Contains(user.Username)) single username
                user.Uid = generateUid(user.Username);
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); //awaits that the db saves the changes

                return Redirect(nameof(Index));
            }

            return BadRequest("All fields need to be filled");
        }



        private static string generateUid(string username)
        {
            return $"{DateTime.Now.ToString("yyyyMMddffffff")}{username}";
        }
    }
}
