using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRequirementService
    {
        Task<List<Requirement>> GetAllRequirementsAsync();
        Task<Requirement?> GetRequirementByIdAsync(int id);
        Task CreateRequirementAsync(RequirementCreateDTO requirementCreateDTO);
        // Task UpdateAccountAsync(int id, AccountUpdateDTO accountUpdateDTO);
    }
}
