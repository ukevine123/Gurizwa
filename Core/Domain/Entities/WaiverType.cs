namespace Domain.Entities
{
    public class WaiverType
    {
        public int Id { get; set; }
        public string? WaiverTypeName { get; set; } // "Penalty Waiver", "Interest Waiver", "Principal Waiver"
        public int LoanProductId { get; set; }
        public LoanProduct LoanProduct { get; set; } = default!;
        
        /// <summary>
        /// The pre-configured outstanding amount for this waiver type
        /// </summary>
        public decimal OutstandingAmount { get; set; }
        
        /// <summary>
        /// Description of the waiver type
        /// </summary>
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
