using System.ComponentModel.DataAnnotations;

namespace Vm.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Team Lead (must be a user with Team Lead role)
        public string TeamLeadId { get; set; }
        public ApplicationUser TeamLead { get; set; }

        // Project the team is working on
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        // Team members
        public ICollection<ApplicationUser> Developers { get; set; }
    }
}
