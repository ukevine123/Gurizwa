using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Guarantor
    {
        public int Id {get;set;}
        public int GuarantorTypeId {get;set;}
        public GuarantorType GuarantorType {get;set;}
        public int PersonId {get;set;}
        public Person Person {get;set;}
        public string? FirstName {get;set;}
        public string? LastName {get;set;}
        [Required(ErrorMessage = "Identification is required")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Identification must be exactly 16 digits")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Identification must contain only numbers")]
        public required string Identification {get;set;}
        public int LoanApplicationId {get;set;}
        public LoanApplication LoanApplication {get;set;}
        public DateTime DateOfBirth {get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
        public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}

        // Organization-specific fields (mirroring Borrower entity)
        public string? CompanyName {get;set;}
        public string? TIN {get;set;}
        public string? ContactPersonName {get;set;}
        public string? ContactPersonPhone {get;set;}
    }
}