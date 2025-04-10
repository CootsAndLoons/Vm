using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vm.Data;
using Vm.Models;
using System.Linq;

namespace Vm.Controllers
{
    public class VacationRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to inject the ApplicationDbContext and UserManager
        public VacationRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
