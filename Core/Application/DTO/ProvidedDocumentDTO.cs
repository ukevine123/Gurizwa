namespace Application.DTO
{
    public class CreateProvidedDocumentDTO
    {
        public int LoanApplicationId {get;set;}        
        public string DocumentName{get;set;}
        public byte[] DocumentFile{get;set;}
        public bool IsPhysicalDocument { get; set; }
    }
}