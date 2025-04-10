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

        // Add validation to ensure that EndDate is after StartDate
        public bool IsValidRequest()
        {
            return EndDate >= StartDate;
        }

        // Additional helper method for checking if the request is a valid full day or half day
        public bool IsValidHalfDay()
        {
            // Ensures that half-day requests have valid "half day" duration logic (this can be extended)
            return !IsHalfDay || (IsHalfDay && StartDate.Date == EndDate.Date);
        }
    }

    public enum VacationType
    {
        Paid,
        Unpaid,
        Sick
    }
}
