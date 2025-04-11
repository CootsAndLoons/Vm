using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vm.Data; // Ensure you include the namespace for ApplicationDbContext
using Vm.Models;

namespace Vm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Add this field to hold your context

        // Inject ApplicationDbContext into the controller's constructor
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Initialize _context with the injected instance
        }

        public IActionResult Index()
        {
            // Get the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);

            // Pass the user's role to the view
            ViewBag.UserRole = user?.Role?.Name;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
