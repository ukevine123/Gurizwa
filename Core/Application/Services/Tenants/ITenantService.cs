using Application.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Tenants
{
    public interface ITenantService
    {
        Task<bool> RegisterTenantAsync(TenantRegistrationDTO dto);
        Task<bool> ApproveTenantAsync(int tenantId);
        Task<bool> RejectTenantAsync(int tenantId);
        Task<List<TenantDTO>> GetPendingTenantsAsync();
        Task<List<TenantDTO>> GetAllTenantsAsync();
        Task<TenantDTO> GetTenantByEmailAsync(string email);
        Task<TenantDTO> GetTenantByIdAsync(int id);
    }
}
