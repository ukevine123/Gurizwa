namespace Domain.Entities
{
    public class ProvidedDocument
    {
        public int Id {get;set;}
        public int LoanApplicationId{get;set;}
        public string DocumentName{get;set;}
        public string DocumentFile{get;set;}
        public DateTime CreatedAt {get;set;}
        public string CreatedBy{get;set;}
        
    }
}