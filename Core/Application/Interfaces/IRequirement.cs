using Application.DTO;
using Application.Interfaces;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IRequirement
    {
        Task<List<Requirement>> GetAllRequirementsAsync();
        Task<Requirement?> GetRequirementByIdAsync(int id);
        Task CreateRequirementAsync(RequirementCreateDTO requirementCreateDTO);
        // Task UpdateAccountAsync(int id, AccountUpdateDTO accountUpdateDTO);
    }
}