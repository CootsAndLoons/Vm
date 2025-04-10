using System;
using System.ComponentModel.DataAnnotations;

namespace Vm.Models
{
    public class LeaveHistory
    {
        public int Id { get; set; }

        [Required]
        public string RequesterId { get; set; }
        public ApplicationUser Requester { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public VacationType LeaveType { get; set; }

        public bool IsApproved { get; set; }

        public DateTime ApprovalDate { get; set; }

        public string ApprovalUserId { get; set; }
        public ApplicationUser ApprovalUser { get; set; }
    }
}
