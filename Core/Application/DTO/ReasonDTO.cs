using System.ComponentModel.DataAnnotations;
namespace Application.DTO
{

    public class CreateReasonDTO
    {
        [Required(ErrorMessage = "Reason is required")]
        [StringLength(100)]
        public string Name { get; set; }
        
    }
}