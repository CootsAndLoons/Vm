using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Vm.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public int? TeamId { get; set; }
        public Team Team { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<LeaveHistory> LeaveHistories { get; set; }

        public ICollection<VacationRequest> VacationRequests { get; set; }

        // Проверка дали потребителят може да одобри заявка за отпуск
        public bool CanApproveVacationRequest(VacationRequest request)
        {
            // Логика: ако потребителят е Team Lead и заявката е за неговия екип
            if (Role?.Name == "Team Lead" && request.Requester.TeamId == TeamId)
            {
                return true;
            }

            // Логика: ако потребителят е CEO, той може да одобри всички заявки
            if (Role?.Name == "CEO")
            {
                return true;
            }

            return false;
        }
    }
}
