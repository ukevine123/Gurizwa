namespace Domain.Entities
{
    public class ProvidedDocument
    {
        public int Id {get;set;}
        public string DocumentName{get;set;}
        public byte[]  DocumentFile{get;set;}
        public DateTime CreatedAt {get;set;}
        public string CreatedBy{get;set;}
        
    }
}