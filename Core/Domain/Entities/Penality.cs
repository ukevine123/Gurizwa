namespace Domain.Entities
{
    public class Penality
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }
        public LoanApplication LoanApplication { get; set; } = default!;
        public int PersonId { get; set; }
        public Person Person { get; set; } 
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int ReasonId { get; set; }
        public Reason Reason { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
}
    }