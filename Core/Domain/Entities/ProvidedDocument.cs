namespace Domain.Entities
{
    public class ProvidedDocument
    {
        public int Id {get;set;}
        public string DocumentName{get;set;}
        public byte[]  DocumentFile{get;set;}
        public int PersonId {get;set;}
        public Person Person {get;set;}
        public int LoanApplicationId {get;set;}
        public LoanApplication LoanApplication {get;set;}
        public DateTime CreatedAt {get;set;}
        public string CreatedBy{get;set;}
        
    }
}