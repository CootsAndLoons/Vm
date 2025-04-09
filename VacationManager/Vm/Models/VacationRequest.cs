using System.ComponentModel.DataAnnotations;

namespace Vm.Models
{
    public class VacationRequest
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public bool IsHalfDay { get; set; }

        public bool IsApproved { get; set; } = false;

        [Required]
        public VacationType Type { get; set; } // Paid, Unpaid, Sick

        // For sick leave only
        public string? FilePath { get; set; }

        // Requester
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
