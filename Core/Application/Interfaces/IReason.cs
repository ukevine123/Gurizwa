using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IReason
    {
        Task<Reason?> GetReasonByIdAsync(int id);

        Task <List<Reason>> GetAllReasonsAsync();
        Task CreateReasonAsync(CreateReasonDTO reasonDTO);
    }
}