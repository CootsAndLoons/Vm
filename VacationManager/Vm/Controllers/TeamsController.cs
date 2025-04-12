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

        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Teams
                  .Include(t => t.Project)
                  .Include(t => t.TeamLead)
                  .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t =>
                    t.Name.Contains(searchString) ||
                    t.Project.Name.Contains(searchString)
                );
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await query.ToListAsync());
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
                    var teamLead = await _context.Users.FindAsync(team.TeamLeadId);
                    if (teamLead == null)
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

                    teamLead.TeamId = team.Id;
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
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
                try
                {
                    var existingTeam = await _context.Teams
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);

                    // Update team first
                    _context.Update(team);
                    await _context.SaveChangesAsync();

                    // Handle team lead changes
                    if (existingTeam?.TeamLeadId != team.TeamLeadId)
                    {
                        // Remove old team lead assignment
                        if (existingTeam?.TeamLeadId != null)
                        {
                            var oldTeamLead = await _context.Users.FindAsync(existingTeam.TeamLeadId);
                            if (oldTeamLead != null)
                            {
                                oldTeamLead.TeamId = null;
                            }
                        }

                        // Assign new team lead
                        var newTeamLead = await _context.Users.FindAsync(team.TeamLeadId);
                        if (newTeamLead != null)
                        {
                            newTeamLead.TeamId = team.Id;
                        }

                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Error updating team. Details: " + ex.Message);
                }
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
            var team = await _context.Teams
                    .Include(t => t.TeamLead)
                    .FirstOrDefaultAsync(t => t.Id == id);

            if (team != null)
            {
                // Remove team lead assignment
                if (team.TeamLead != null)
                {
                    team.TeamLead.TeamId = null;
                }

                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Project)
                .Include(t => t.TeamLead)
                .Include(t => t.Developers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            ViewBag.AvailableUsers = new SelectList(
                _context.Users
                    .Where(u => u.TeamId == null || u.TeamId == id)
                    .Include(u => u.Role),
                "Id",
                "UserName"
            );

            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int teamId, string userId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            var user = await _context.Users.FindAsync(userId);

            if (team != null && user != null)
            {
                user.TeamId = teamId;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int teamId, string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TeamId = null;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = teamId });
        }
    }
}
