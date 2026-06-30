namespace Domain.Entities
{
    public class Waiver
    {
        public int Id { get; set; }
        public int DisbursementId { get; set; }
        public Disbursement Disbursement { get; set; } = default!;
        
        public int WaiverTypeId { get; set; }
        public WaiverType WaiverType { get; set; } = default!;
        
        public string WaiverTypeName { get; set; } // "FeeWaiver" or "LoanWaiveOff"

        /// <summary>
        /// The loan component being waived: "Penalty", "Interest", or "Principal"
        /// </summary>
        public string Component { get; set; } = "Penalty";
        
        /// <summary>
        /// Amount being waived (from WaiverType.OutstandingAmount)
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Reason for the waiver (e.g., "Natural Disaster", "Government Initiative", "Goodwill")
        /// </summary>
        public string Reason { get; set; }
        
        /// <summary>
        /// Detailed description of the waiver
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Status of the waiver: "Pending", "Approved", "Rejected"
        /// </summary>
        public string Status { get; set; } = "Pending";
        
        /// <summary>
        /// Who approved the waiver
        /// </summary>
        public string? ApprovedBy { get; set; }
        
        /// <summary>
        /// Date the waiver was approved
        /// </summary>
        public DateTime? ApprovedDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
