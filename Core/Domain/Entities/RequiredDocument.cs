using Domain.Entities;

namespace Domain.Entities
{
    public class RequiredDocument
    {
        public int Id { get; set; }
        public string? DocumentName { get; set; }
        public string? DocumentType { get; set; }

    }
}
