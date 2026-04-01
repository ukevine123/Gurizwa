namespace Domain.Entities
{
public class Payment
{
    public int Id { get; set; }
    public int DisbursementId { get; set; }
    public Disbursement Disbursement { get; set; } = default!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public int PaymentTypeId { get; set; }
    public PaymentType PaymentType { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
}
