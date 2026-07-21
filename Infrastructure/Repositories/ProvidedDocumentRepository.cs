using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class ProvidedDocumentRepository : IProvidedDocument
    {

  private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
  private readonly IUserContext _userContext;
      
         public ProvidedDocumentRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
           _contextFactory = contextFactory;
           _userContext = userContext;
        }
        public async Task<List<ProvidedDocument>> GetAllProvidedDocumentAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            if (_userContext.Id == null)
            {
                return new List<ProvidedDocument>();
            }
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            return await dbContext.ProvidedDocuments
               .AsNoTracking()
               .Where(a => allowedPersonIds.Contains(a.PersonId))
               .Select(a => new ProvidedDocument {
                   Id = a.Id,
                   DocumentName = a.DocumentName,
                   PersonId = a.PersonId,
                   Person = a.Person != null ? new Person {
                       Id = a.Person.Id,
                       FirstName = a.Person.FirstName,
                       LastName = a.Person.LastName,
                       CompanyName = a.Person.CompanyName,
                       TinNumber = a.Person.TinNumber,
                       ContactPerson = a.Person.ContactPerson
                   } : null,
                   LoanApplicationId = a.LoanApplicationId,
                   LoanApplication = a.LoanApplication != null ? new LoanApplication {
                       Id = a.LoanApplication.Id,
                       ApplicationCode = a.LoanApplication.ApplicationCode,
                       BorrowerId = a.LoanApplication.BorrowerId,
                       Borrower = a.LoanApplication.Borrower != null ? new Borrower {
                           Id = a.LoanApplication.Borrower.Id,
                           CompanyName = a.LoanApplication.Borrower.CompanyName,
                           FirstName = a.LoanApplication.Borrower.FirstName,
                           LastName = a.LoanApplication.Borrower.LastName,
                           ContactPersonName = a.LoanApplication.Borrower.ContactPersonName
                       } : null,
                       PersonId = a.LoanApplication.PersonId,
                       Person = a.LoanApplication.Person != null ? new Person {
                           Id = a.LoanApplication.Person.Id,
                           CompanyName = a.LoanApplication.Person.CompanyName,
                           FirstName = a.LoanApplication.Person.FirstName,
                           LastName = a.LoanApplication.Person.LastName,
                           ContactPerson = a.LoanApplication.Person.ContactPerson
                       } : null
                   } : null,
                   CreatedAt = a.CreatedAt,
                   CreatedBy = a.CreatedBy
               })
               .ToListAsync(); 
        }
        public async Task <ProvidedDocument> GetProvidedDocumentById(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            if (_userContext.Id == null)
            {
                return null;
            }
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            return await dbContext.ProvidedDocuments
            .Where(a => allowedPersonIds.Contains(a.PersonId)) 
            .Include(d => d.LoanApplication)
            .FirstOrDefaultAsync(t => t.Id == Id);
        }
        public async Task<ProvidedDocument> CreateProvidedDocument(CreateProvidedDocumentDTO dto)
       {
            if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }

            using var dbContext = await _contextFactory.CreateDbContextAsync();

    // 2. Query 'Users' from the dbContext instance, NOT the factory
         var user = await dbContext.Users
        .Include(u => u.Person) // You'll likely need this to link the account
        .FirstOrDefaultAsync(u => u.Id == _userContext.Id);

            if (user == null)
            {
                throw new Exception("User record not found");
            }

            if (user.Person == null)
            {
                throw new Exception("Authenticated user does not have an associated Person record.");
            }
       

        var loanApplication = await dbContext.LoanApplications.FindAsync(dto.LoanApplicationId);
        
        var newDoc = new ProvidedDocument
        {
            LoanApplication = loanApplication,
            DocumentName = dto.DocumentName,
            PersonId = user.Person.Id,
            DocumentFile = dto.DocumentFile, 
            IsPhysicalDocument = dto.IsPhysicalDocument,
            CreatedAt = DateTime.Now,
            CreatedBy = "Admin" 
        };
        // Add and Save using the SAME dbContext instance
        dbContext.ProvidedDocuments.Add(newDoc);
        await dbContext.SaveChangesAsync(); 

        return newDoc; 
     }

     public async Task DeleteProvidedDocumentAsync(int id)
     {
         using var dbContext = await _contextFactory.CreateDbContextAsync();
         var doc = await dbContext.ProvidedDocuments.FindAsync(id);
         if (doc != null)
         {
             dbContext.ProvidedDocuments.Remove(doc);
             await dbContext.SaveChangesAsync();
         }
     }
    }
}
        