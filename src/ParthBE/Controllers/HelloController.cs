using ParthBE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ParthBE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ParthBEDbContext _context;

        public HomeController(ParthBEDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userCount = await _context.Users.CountAsync();
            ViewData["UserCount"] = userCount;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}