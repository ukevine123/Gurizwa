using System.ComponentModel.DataAnnotations;
namespace Application.DTO
{
    public class CreatePenalityDTO
    {
     [Required(ErrorMessage = "Please select an LoanApplication")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select an Expense type")]
       public int LoanApplicationId { get; set; }   // ✅ correct property

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
       public decimal Amount { get; set; }

            [Required(ErrorMessage = "Please select an Expense type")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select an Expense type")]
       public DateTime? Date { get; set; } = DateTime.Today;  // ✅ correct property

       [Required(ErrorMessage = "Please select an LoanApplication")]
      [Range(1, int.MaxValue, ErrorMessage = "Please select an Expense type")]
       public int ReasonId { get; set; }   // ✅ correct property

        [Required(ErrorMessage = "Description is required")]
        [StringLength(100)]
        public string Description { get; set; }
    }
}