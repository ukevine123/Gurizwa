using System.ComponentModel.DataAnnotations;
namespace Application.DTO
{
    public class CreatePaymentDTO
    {
     [Required(ErrorMessage = "Please select Disbursement")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select an  disbursement")]
       public int DisbursementId  { get; set; }   // ✅ correct property

        [Required(ErrorMessage = "Please select Account")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select Account")]
       public int AccountId   { get; set; }   // ✅ correct property

         [Required(ErrorMessage = "Please select PaymentType")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select PaymentType")]
       public int PaymentTypeId   { get; set; }   // ✅ correct property

       [Required(ErrorMessage = "Amount is required")]
     [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
       public decimal Amount { get; set; }
     


    [Required(ErrorMessage = "Date is required")]
    public DateTime? PaymentDate  { get; set; } = DateTime.Today; // Defaults to Today's date     
    }
}