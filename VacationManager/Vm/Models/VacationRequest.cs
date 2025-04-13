using System.ComponentModel.DataAnnotations;

namespace Vm.Models
{
    public class VacationRequest
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public bool IsHalfDay { get; set; }

        public bool IsApproved { get; set; }

        [Required]
        public VacationType Type { get; set; }

        // Removed file-related properties
        public string RequesterId { get; set; }
        public ApplicationUser Requester { get; set; }
    }

    public enum VacationType
    {
        Paid,
        Unpaid,
        Sick
    }
}
