using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public class WaiverTypeDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Waiver type name is required")]
        public string WaiverTypeName { get; set; } // "Penalty Waiver", "Interest Waiver", "Principal Waiver"
        
        [Required(ErrorMessage = "Loan product is required")]
        public int LoanProductId { get; set; }
        
        [Required(ErrorMessage = "Outstanding amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Outstanding amount must be greater than 0")]
        public decimal OutstandingAmount { get; set; }
        
        public string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateWaiverTypeDTO
    {
        [Required(ErrorMessage = "Waiver type name is required")]
        public string WaiverTypeName { get; set; } // "Penalty Waiver", "Interest Waiver", "Principal Waiver"
        
        [Required(ErrorMessage = "Loan product is required")]
        public int LoanProductId { get; set; }
        
        [Required(ErrorMessage = "Outstanding amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Outstanding amount must be greater than 0")]
        public decimal OutstandingAmount { get; set; }
        
        public string Description { get; set; }
    }
}
