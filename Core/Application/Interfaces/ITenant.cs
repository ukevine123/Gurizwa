using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITenant
    {
        Task<Tenant> GetTenantByEmailAsync(string email);
        Task<Tenant> GetTenantByIdAsync(int id);
        Task<List<Tenant>> GetPendingTenantsAsync();
        Task<List<Tenant>> GetAllTenantsAsync();
        Task AddTenantAsync(Tenant tenant);
        Task UpdateTenantAsync(Tenant tenant);
    }
}
