using System.ComponentModel.DataAnnotations;


namespace Application.DTO
{
    public class TenantRegistrationDTO
    {
        [Required]
        public string TenantType { get; set; } = "Business";
        
        public string? CompanyName { get; set; }
        
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        [Required]
        public string PhoneNumber { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        public string? Location { get; set; }
        
        public List<TenantDocumentUploadDTO> Documents { get; set; } = new();
    }

    public class TenantDocumentUploadDTO
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
    }

    public class TenantDTO
    {
        public int Id { get; set; }
        public string TenantType { get; set; }
        public string? CompanyName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? Location { get; set; }
        public bool IsApproved { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TenantDocumentDTO> Documents { get; set; } = new();
    }

    public class TenantDocumentDTO
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public byte[] DocumentFile { get; set; }
    }
}
