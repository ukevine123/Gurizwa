using Domain.ValueObjects;
namespace Application.DTO
{
    public class CreateProcessFeeDepositDTO
    {
    public int LoanApplicationId { get; set; }
    public decimal  Amount { get; set; }
  
    public int AccountId { get; set; }
    public DateTime DepositDate { get; set; }
    public  FeeDepositStatus Status {get;set;}
  
    }
    public class UpdateProcessFeeDepositDTO
    {
   public decimal  Amount { get; set; }
   
    public int AccountId { get; set; }
    public DateTime DepositDate { get; set; }
     public  FeeDepositStatus Status {get;set;}
    }
}