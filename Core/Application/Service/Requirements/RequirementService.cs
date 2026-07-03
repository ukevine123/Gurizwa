using System.Security.Cryptography.X509Certificates;
using Application.Interfaces;
using Domain.Entities;
using Application.DTO;


namespace Application.Services.Requirements
{
    
    public class RequirementService : IRequirementService
    {
        private readonly IRequirement _requirement;

        //Constructor
        public RequirementService(IRequirement requirement)
        {
            _requirement = requirement;
        }
        
        public async Task<List<Requirement>> GetAllRequirementsAsync()
        {
            return await _requirement.GetAllRequirementsAsync();
        }

        public async Task<Requirement?> GetRequirementByIdAsync(int id)
        {
            return await _requirement.GetRequirementByIdAsync(id);
        }   

        public async Task CreateRequirementAsync(RequirementCreateDTO requirementDTO)
        {  
            await _requirement.CreateRequirementAsync(requirementDTO);
        }

        // public async Task UpdateAccountAsync(int id, AccountUpdateDTO accountDTO)
        // {
        //     await _account.UpdateAccountAsync(id, accountDTO);
        // }
    }
}