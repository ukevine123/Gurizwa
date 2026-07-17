using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Tenant
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TenantType { get; set; } // e.g. "Organization" or "Personal"
        
        [StringLength(200)]
        public string? CompanyName { get; set; }
        
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }
        
        [StringLength(500)]
        public string? Location { get; set; }
        
        public bool IsApproved { get; set; } = false;
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TenantDocument> TenantDocuments { get; set; } = new List<TenantDocument>();
    }
}
