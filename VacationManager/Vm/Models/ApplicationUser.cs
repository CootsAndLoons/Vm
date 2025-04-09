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

        public ICollection<VacationRequest> VacationRequests { get; set; }
    }
}
