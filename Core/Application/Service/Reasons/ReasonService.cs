using Domain.Entities;
using Application.DTO;
using Application.Interfaces;

namespace Application.Services.Reasons
{
    public class ReasonService : IReasonService
    {

    private readonly IReason _reason;

    public ReasonService(IReason reason)
    {
        _reason = reason;
    }
   
       public async Task<Reason?> GetReasonByIdAsync(int id)
    {
        return await _reason.GetReasonByIdAsync(id);
        
    }

     public async Task<List<Reason>> GetAllReasonsAsync()
        {
            return await _reason.GetAllReasonsAsync();
        }

        public async Task CreateReasonAsync(CreateReasonDTO reasonDTO)
        {
             await _reason.CreateReasonAsync(reasonDTO);
        }

     

}
}