namespace Application.DTO
{
    public class CreateGuarantorDTO
    {
        public int GuarantorTypeId {get;set;}
        public string FirstName {get;set;}
        public string LastName {get;set;}
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
    }
    public class UpdateGuarantorDTO
    {
        public string FirstName {get;set;}
        public string LastName {get;set;}
        public string Identification {get;set;}
        public DateTime DateOfBirth {get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
       public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;} 
    }
}