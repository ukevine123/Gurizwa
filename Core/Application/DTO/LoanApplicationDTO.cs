using Domain.ValueObjects;
namespace Application.DTO
{
    public class CreateApplicationDTO
    {
         public int LoanProductSettingId {get;set;}
        public int BorrowerId {get;set;}
        public int PaymentModalityId{get;set;}
        public  int ProvidedDocumentId {get;set;}
        public decimal AmountRequested {get;set;}
        public DateTime DateofApplication{get;set;}
        public  LoanStatus Status {get;set;}
        public DateTime PreferredDate{get;set;}
        public string ApprovedBy {get;set;}
    }
    public class UpdateApplicationDTO
    {
        public int PaymentModalityId{get;set;}
        public decimal AmountRequested {get;set;}  
        public string ApprovedBy {get;set;}
        public  LoanStatus Status {get;set;}
        public DateTime PreferredDate{get;set;}
        public DateTime DateofApplication{get;set;}
    }
}