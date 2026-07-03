using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public class WaiverDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Disbursement is required")]
        public int DisbursementId { get; set; }
        
        [Required(ErrorMessage = "Waiver type is required")]
        [RegularExpression("^(Penalty Waiver|Interest Waiver|Principal Waiver)$", ErrorMessage = "Waiver type must be Penalty Waiver, Interest Waiver, or Principal Waiver.")]
        public string WaiverType { get; set; } // "Penalty Waiver", "Interest Waiver", or "Principal Waiver"

        public string Component { get; set; } // "Penalty", "Interest", or "Principal"
        
        // Amount is auto-calculated based on waiver type outstanding amount
        public decimal Amount { get; set; }
        
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        
        public string Description { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public string ApprovedBy { get; set; }
        
        public DateTime? ApprovedDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateWaiverDTO
    {
        [Required(ErrorMessage = "Disbursement is required")]
        public int DisbursementId { get; set; }
        
        [Required(ErrorMessage = "Waiver type is required")]
        [RegularExpression("^(Penalty Waiver|Interest Waiver|Principal Waiver)$", ErrorMessage = "Waiver type must be Penalty Waiver, Interest Waiver, or Principal Waiver.")]
        public string WaiverType { get; set; } // "Penalty Waiver", "Interest Waiver", or "Principal Waiver"

        public string Component { get; set; } // "Penalty", "Interest", or "Principal"
        
        // Amount is auto-calculated based on waiver type outstanding amount - optional for client
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
        
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        
        public string Description { get; set; }
    }

    public class UpdateWaiverStatusDTO
    {
        [Required]
        public int WaiverId { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } // "Approved" or "Rejected"
        
        public string ApprovedBy { get; set; }
        
        public string RejectionReason { get; set; }
    }
}
