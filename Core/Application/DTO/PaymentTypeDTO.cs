using System.ComponentModel.DataAnnotations;
namespace Application.DTO
{

    public class CreatePaymentTypeDTO
    {
        [Required(ErrorMessage = "Payment Type Name is required")]
        [StringLength(100)]
        public string PaymentTypeName { get; set; }
        
    }
}