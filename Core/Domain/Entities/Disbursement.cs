namespace Domain.Entities
{
    public class Disbursement
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }
        public LoanApplication LoanApplication { get; set; } = default!;
        public int PaymentModalityId { get; set; }
        public PaymentModality PaymentModality { get; set; } = default!;
        public int TotalInstallments { get; set; }
        public decimal PrincipalOffered { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // It allows .Include(i => i.Payments) to work.
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
       
         public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}   