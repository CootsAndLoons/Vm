using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vm.Data;
using Vm.Models;
using System.Linq;
using X.PagedList.Mvc;
using Microsoft.AspNetCore.Authorization;
using X.PagedList.Extensions;

namespace Vm.Controllers
{
    [Authorize]
    public class VacationRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public VacationRequestController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: VacationRequest
        public async Task<IActionResult> Index(DateTime? filterDate, int? page)
        {
            var user = await _userManager.GetUserAsync(User);
            var query = _context.VacationRequests
                .Include(v => v.Requester)
                .Where(v => v.RequesterId == user.Id)
                .OrderByDescending(v => v.CreatedOn)
                .AsQueryable();

            if (filterDate.HasValue)
            {
                query = query.Where(v => v.CreatedOn >= filterDate.Value);
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var filteredRequests = await query.ToListAsync();
            return View(filteredRequests.ToPagedList(pageNumber, pageSize));
        }

        // GET: VacationRequest/Create
        public IActionResult Create()
        {
            return View(new VacationRequest
            {
                CreatedOn = DateTime.Now,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            });
        }

        // POST: VacationRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VacationRequest model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.RequesterId = user.Id; // Set programmatically
            model.CreatedOn = DateTime.Now;

            // Remove RequesterId/Requester from ModelState validation
            ModelState.Remove("RequesterId");
            ModelState.Remove("Requester");

            if (model.Type == VacationType.Sick)
            {
                model.IsHalfDay = false;
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving request: {ex.Message}");
                }
            }

            // Repopulate dates if validation fails
            model.StartDate = model.StartDate == DateTime.MinValue ? DateTime.Today : model.StartDate;
            model.EndDate = model.EndDate == DateTime.MinValue ? DateTime.Today : model.EndDate;

            return View(model);
        }

        // GET: VacationRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var request = await _context.VacationRequests
                .Include(v => v.Requester)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (request == null || request.RequesterId != _userManager.GetUserId(User) || request.IsApproved)
            {
                return Forbid();
            }

            return View(request);
        }

        // POST: VacationRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VacationRequest model) // Remove IFormFile sickLeaveFile parameter
        {
            var existingRequest = await _context.VacationRequests.FindAsync(id);
            if (existingRequest == null || existingRequest.RequesterId != _userManager.GetUserId(User) || existingRequest.IsApproved)
            {
                return Forbid();
            }

            ModelState.Remove("RequesterId");
            ModelState.Remove("Requester");

            if (model.Type == VacationType.Sick)
            {
                model.IsHalfDay = false;
                // Remove file validation
            }

            if (ModelState.IsValid)
            {
                existingRequest.StartDate = model.StartDate;
                existingRequest.EndDate = model.EndDate;
                existingRequest.Type = model.Type;
                existingRequest.IsHalfDay = model.IsHalfDay;

                // Remove file handling code
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: VacationRequest/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.VacationRequests.FindAsync(id);
            if (request == null || request.RequesterId != _userManager.GetUserId(User) || request.IsApproved)
            {
                return Forbid();
            }

            _context.VacationRequests.Remove(request);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Team Lead,CEO")]
        public async Task<IActionResult> PendingApprovals(int? page)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            IQueryable<VacationRequest> query = _context.VacationRequests
                .Include(v => v.Requester)
                .ThenInclude(u => u.Team)
                .Where(v => !v.IsApproved);

            if (await _userManager.IsInRoleAsync(currentUser, "Team Lead"))
            {
                // Team Lead can only see requests from their team
                query = query.Where(v => v.Requester.TeamId == currentUser.TeamId);
            }
            // CEO can see all requests

            int pageSize = 10;
            int pageNumber = page ?? 1;

            var filteredRequests = await query.ToListAsync();
            return View(filteredRequests.ToPagedList(pageNumber, pageSize));
        }


        // Action to approve a vacation request
        public async Task<IActionResult> Approve(int vacationRequestId)
        {
            var vacationRequest = await _context.VacationRequests
                .Include(v => v.Requester) // Get the user who requested the vacation
                .FirstOrDefaultAsync(v => v.Id == vacationRequestId);

            if (vacationRequest == null)
            {
                return NotFound();
            }

            // Get the current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if the user has permission to approve the vacation request
            if (!currentUser.CanApproveVacationRequest(vacationRequest))
            {
                return Unauthorized();
            }

            // Mark the vacation request as approved
            vacationRequest.IsApproved = true;
            await _context.SaveChangesAsync();

            // Create a LeaveHistory entry to log the approval
            var leaveHistory = new LeaveHistory
            {
                RequesterId = vacationRequest.RequesterId,
                StartDate = vacationRequest.StartDate,
                EndDate = vacationRequest.EndDate,
                LeaveType = vacationRequest.Type,
                IsApproved = true,
                ApprovalDate = DateTime.Now,
                ApprovalUserId = currentUser.Id,
                ApprovalUser = currentUser
            };

            // Add the leave history to the database
            _context.LeaveHistories.Add(leaveHistory);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // Redirect to the home page or any relevant page
        }

        // Action to reject a vacation request (optional)
        public async Task<IActionResult> Reject(int vacationRequestId)
        {
            var vacationRequest = await _context.VacationRequests
                .FirstOrDefaultAsync(v => v.Id == vacationRequestId);

            if (vacationRequest == null)
            {
                return NotFound();
            }

            // Get the current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if the user has permission to reject the vacation request
            if (!currentUser.CanApproveVacationRequest(vacationRequest))
            {
                return Unauthorized();
            }

            // Mark the vacation request as rejected (you can add a rejected status if needed)
            vacationRequest.IsApproved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // Redirect to the home page or any relevant page
        }
    }
}
