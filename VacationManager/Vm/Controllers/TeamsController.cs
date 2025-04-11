using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vm.Data;
using Vm.Models;

namespace Vm.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams.AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.TeamLead)
                .ToListAsync();


            return View(teams);
        }


        public IActionResult Create()
        {
            // Попълване на ViewBag за проекти и лидери на екипи
            ViewBag.Projects = new SelectList(_context.Projects, "Id", "Name");
            ViewBag.TeamLeads = new SelectList(
                _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.Name == "Team Lead"),
                "Id",
                "UserName" // Или FirstName + LastName
            );

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verify TeamLeadId exists
                    var teamLeadExists = await _context.Users.AnyAsync(u => u.Id == team.TeamLeadId);
                    if (!teamLeadExists)
                    {
                        ModelState.AddModelError("TeamLeadId", "Selected Team Lead does not exist.");
                        return View(team);
                    }

                    // Verify ProjectId exists
                    var projectExists = await _context.Projects.AnyAsync(p => p.Id == team.ProjectId);
                    if (!projectExists)
                    {
                        ModelState.AddModelError("ProjectId", "Selected Project does not exist.");
                        return View(team);
                    }

                    _context.Add(team);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Log the full error (ex.InnerException?.Message)
                    ModelState.AddModelError("", "Error saving team. Details: " + ex.Message);
                }
            }

            // Repopulate dropdowns on error
            ViewBag.Projects = new SelectList(_context.Projects, "Id", "Name");
            ViewBag.TeamLeads = new SelectList(
                _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.Name == "Team Lead"),
                "Id",
                "UserName"
            );
            return View(team);
            
        }

        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            // Подаваме проектите и лидерите на екипи чрез ViewBag
            ViewBag.Projects = new SelectList(_context.Projects, "Id", "Name", team.ProjectId);
            ViewBag.TeamLeads = new SelectList(
                _context.Users.Where(u => u.Role.Name == "Team Lead"),
                "Id",
                "UserName",
                team.TeamLeadId
            );

            return View(team);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Team team)
        {
            if (id != team.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Project)
                .Include(t => t.TeamLead)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
