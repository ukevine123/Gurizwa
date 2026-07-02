using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReasonRepository : IReason
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IUserContext _userContext;    

        public ReasonRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task<List<Reason>> GetAllReasonsAsync()
        {
            if (_userContext.Id == null)
            {
                return new List<Reason>();
            }       
            using var context = _contextFactory.CreateDbContext();
            return await context.Reasons
            .Where(a => a.PersonId == _userContext.PersonId) 
            .ToListAsync();
        }

        public async Task<Reason?> GetReasonByIdAsync(int id)
        {
            if (_userContext.Id == null)
            {
                return new Reason();
            }
            using var context = _contextFactory.CreateDbContext();
            return await context.Reasons.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task CreateReasonAsync(CreateReasonDTO reasonDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            
            if (_userContext.Id == null) throw new Exception("User not authenticated");
            var user = await context.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            if (user == null || user.Person == null) throw new Exception("User or Person not found.");

            var reason = new Reason
            {
                Name = reasonDTO.Name,
                IsActive = true,
                PersonId = user.Person.Id
            };

            context.Reasons.Add(reason);
            await context.SaveChangesAsync();
        }
    }
}