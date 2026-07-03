using Domain.Entities;
using Application.DTO;

namespace Application.Services.Reasons
{
    public interface IReasonService
    {
         Task<Reason?> GetReasonByIdAsync(int id);

        Task <List<Reason>> GetAllReasonsAsync();
        Task CreateReasonAsync(CreateReasonDTO reasonDTO);

    }
}