using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class TenantDocument
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string DocumentName { get; set; }
        
        [Required]
        public byte[] DocumentFile { get; set; }
        
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
