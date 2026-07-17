using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TenantRepository : ITenant
    {
        private readonly ApplicationDbContext _dbContext;

        public TenantRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Tenant> GetTenantByEmailAsync(string email)
        {
            return await _dbContext.Tenants
                .Include(t => t.TenantDocuments)
                .FirstOrDefaultAsync(t => t.Email == email);
        }

        public async Task<Tenant> GetTenantByIdAsync(int id)
        {
            return await _dbContext.Tenants
                .Include(t => t.TenantDocuments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Tenant>> GetPendingTenantsAsync()
        {
            return await _dbContext.Tenants
                .Include(t => t.TenantDocuments)
                .Where(t => t.Status == "Pending")
                .ToListAsync();
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _dbContext.Tenants
                .Include(t => t.TenantDocuments)
                .ToListAsync();
        }

        public async Task AddTenantAsync(Tenant tenant)
        {
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateTenantAsync(Tenant tenant)
        {
            _dbContext.Tenants.Update(tenant);
            await _dbContext.SaveChangesAsync();
        }
    }
}
