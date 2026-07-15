

namespace Domain.Entities
{
    public class Person
    {
        public int Id {get;set;}
        public string FirstName{get;set;}
        public string LastName{get;set;}
        public string Sex{get;set;}
        public DateTime DateOfBirth{get;set;}
        public string phoneNumber{get;set;}
        public string Email{get;set;}
        public string Country{get;set;}
        public string Status{get;set;}

        // Organization specific fields
        public string TenantType { get; set; }
        public string CompanyName { get; set; }
        public string TinNumber { get; set; }
        public string ContactPerson { get; set; }

            //mult tenant 
        public List<Account> Accounts {get;set;}
        public List<LoanApplication> LoanApplications {get;set;}
        public List<LoanProduct> LoanProducts {get;set;}
        public List<LoanProductSetting> LoanProductSettings {get;set;}
        public List<Borrower> Borrowers {get;set;}
        public List<Guarantor> Guarantors {get;set;}
        public List<Collateral> Collaterals {get;set;}
        public List<ProvidedDocument> ProvidedDocuments {get;set;}
        public List<RequiredDocument> RequiredDocuments {get;set;}
        public List<Requirement> Requirements {get;set;}
        public List<ProcessFeeDeposit> ProcessFeeDeposits {get;set;}
        public List<Penality> Penalities {get;set;}
        public List<Payment> Payments {get;set;}
        public List<Disbursement> Disbursements {get;set;}
       
        public string CreatedBy {get;set;}
        public DateTime CreatedAt {get;set;}
        public string UpdateBy {get;set;}
         

    

    }
}