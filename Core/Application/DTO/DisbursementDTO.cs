using System.ComponentModel.DataAnnotations;
namespace Application.DTO
{
    public class CreateDisbursementDTO
    {
     [Required(ErrorMessage = "Please select Loan Application")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select an Expense type")]
       public int LoanApplicationId  { get; set; }   // ✅ correct property

        [Required(ErrorMessage = "Please select Payment Modality")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select Payment Modality")]
       public int PaymentModalityId   { get; set; }   // ✅ correct property

        [Required(ErrorMessage = "Total installments is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Total installments must be a positive integer")]
        public int TotalInstallments { get; set; }


    [Required(ErrorMessage = "Principal Offered  is required")]
     [Range(0.01, double.MaxValue, ErrorMessage = "Principal Offered  must be greater than 0")]
       public decimal PrincipalOffered  { get; set; }

    [Required(ErrorMessage = "Interest rate is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Interest rate must be greater than 0")]
    public decimal InterestRate  { get; set; }

     [Required(ErrorMessage = "Amount is required")]
     [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
       public decimal Amount { get; set; }


    [Required(ErrorMessage = "Date is required")]
    public DateTime? StartDate  { get; set; } = DateTime.Today; // Defaults to Today's date

    [Required(ErrorMessage = "Date is required")]
     public DateTime? EndDate  { get; set; } = DateTime.Today; 

     
        
    }
}