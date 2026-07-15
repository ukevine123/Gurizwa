namespace Application.DTO
{
    public class CreateGuarantorDTO
    {
        public int GuarantorTypeId {get;set;}
        public string? FirstName {get;set;}
        public string? LastName {get;set;}
        public string Identification {get;set;}
        public int LoanApplicationId {get;set;}
        public DateTime DateOfBirth {get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
        public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}
        public string CreatedBy {get;set;}

        // Organization-specific fields (mirroring Borrower DTO)
        public string? CompanyName {get;set;}
        public string? TIN {get;set;}
        public string? ContactPersonName {get;set;}
        public string? ContactPersonPhone {get;set;}
    }
    public class UpdateGuarantorDTO
    {
        public string? FirstName {get;set;}
        public string? LastName {get;set;}
        public string Identification {get;set;}
        public DateTime DateOfBirth {get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
        public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}

        // Organization-specific fields (mirroring Borrower DTO)
        public string? CompanyName {get;set;}
        public string? TIN {get;set;}
        public string? ContactPersonName {get;set;}
        public string? ContactPersonPhone {get;set;}
    }
}