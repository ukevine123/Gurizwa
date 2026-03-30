namespace Domain.Entities
{

public class ProcessFeeDeposit
{
    public int Id { get; set; }
    public int LoanApplicationId { get; set; }
    public LoanApplication LoanApplication { get; set; } = default!;
    public decimal  Amount { get; set; }
    public int PaymentTypeId { get; set; }
    public PaymentType PaymentType { get; set; } = default!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public DateTime DepositDate { get; set; }
    public decimal ClientAccount { get; set; }
    public bool IsActive { get; set; }
     public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}

}