using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Application.Services.Penalities;

namespace Application.Services.Penalities
{

    public interface IPenalityService
    {
         Task <List<Penality>> GetAllPenalitiesAsync();
      Task <Penality?> GetPenalityByIdAsync(int id);
      Task CreatePenalityAsync(CreatePenalityDTO penalityDTO);
      Task GenerateAutomaticPenalitiesAsync();
    }
   
}