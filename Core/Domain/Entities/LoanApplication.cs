using Domain.ValueObjects;
namespace Domain.Entities
{
     public class LoanApplication
    {
        public int Id  {get;set;}
        public int LoanProductSettingId {get;set;}
        public LoanProductSetting LoanProductSetting {get;set;}
        // public Person Person {get;set;}
        // public int PersonId {get;set;}
        public int BorrowerId {get;set;}
        public Borrower Borrower {get;set;}
        public int PaymentModalityId{get;set;}
        public PaymentModality PaymentModality {get;set;}
        public int providedDocumentId {get;set;}
        public ProvidedDocument ProvidedDocument {get;set;}
        public decimal AmountRequested {get;set;}
        public DateTime DateofApplication{get;set;}
        public  LoanStatus Status {get;set;}
        public DateTime PreferredDate{get;set;}
        public DateTime CreatedAt {get;set;} =DateTime.Now;
        public string ApprovedBy {get;set;} 
        public string CreatedBy {get;set;}

    }
}