using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vm.Data;
using Vm.Models;
using X.PagedList.Extensions;
namespace Vm.Controllers
{
    [Authorize(Policy = "CEOOnly")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 10;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, string roleFilter, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["RoleFilter"] = roleFilter;

            var users = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Team)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u =>
                    u.UserName.Contains(searchString) ||
                    u.FirstName.Contains(searchString) ||
                    u.LastName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(roleFilter))
            {
                users = users.Where(u => u.Role.Name == roleFilter);
            }

            var roles = await _context.Roles.Select(r => r.Name).Distinct().ToListAsync();
            ViewBag.Roles = new SelectList(roles);

            int pageNumber = page ?? 1;
            var filteredUsers = await users.ToListAsync();

            return View(filteredUsers.ToPagedList(pageNumber, PageSize));
        }
        public async Task<IActionResult> Details(string id)
        {
            var user = await _context.Users
                .Include(u => u.Team)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Teams = new SelectList(_context.Teams, "Id", "Name", user.TeamId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeam(string id, int? teamId)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.TeamId = teamId;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
