namespace Domain.Entities
{
public class Payment
{
    public int Id { get; set; }
    public int DisbursementId { get; set; }
    public Disbursement Disbursement { get; set; } = default!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public int PersonId { get; set; }
    public Person Person { get; set; } 
    public int PaymentTypeId { get; set; }
    public PaymentType PaymentType { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal PrincipalPaid { get; set; } = 0;
    public decimal InterestPaid { get; set; } = 0;
    public decimal PenaltyPaid { get; set; } = 0;
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = "Completed"; // "Completed", "Partial", "Void"
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
}