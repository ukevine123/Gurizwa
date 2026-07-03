using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IPenality
    {
      Task <List<Penality>> GetAllPenalitiesAsync();
      Task <Penality?> GetPenalityByIdAsync(int id);
      Task CreatePenalityAsync(CreatePenalityDTO penalityDTO);

        
    }
}