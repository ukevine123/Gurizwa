namespace Domain.Entities
{
    public class Guarantor
    {
        public int Id {get;set;}
        public int GuarantorTypeId {get;set;}
        public GuarantorType GuarantorType {get;set;}
        public string FirstName {get;set;}
        public string LastName {get;set;}
        public string Identification {get;set;}
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
        public DateTime CreatedAt {get;set;}
        public string CreatedBy {get;set;} 

    }
}