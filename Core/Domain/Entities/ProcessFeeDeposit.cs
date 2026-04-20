 using Domain.ValueObjects;
namespace Domain.Entities
  {
public class ProcessFeeDeposit
{
    public int Id { get; set; }
    public int LoanApplicationId { get; set; } 
    public LoanApplication LoanApplication { get; set; }
    public decimal  Amount { get; set; }
    public int PaymentTypeId { get; set; }
    public PaymentType PaymentType { get; set; } 
    public int AccountId { get; set; }
    public Account Account { get; set; }
    public DateTime DepositDate { get; set; }
     public int BorrowerId { get; set; }
    public Borrower Borrower { get; set; }
     public  FeeDepositStatus Status {get;set;}
     public DateTime CreatedAt { get; set; } = DateTime.Now;
     public int? CreatedBy { get; set; }
  
}

}