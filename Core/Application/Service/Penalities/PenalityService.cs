using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
namespace Application.Services.Penalities
{
    public class PenalityService : IPenalityService
    {
        private readonly IPenality _penality;

        public PenalityService(IPenality penality)
        {
            _penality = penality;
        }

        public async Task <Penality?> GetPenalityByIdAsync(int id)
        {
            return await _penality.GetPenalityByIdAsync(id);
        }

        public async Task<List<Penality>> GetAllPenalitiesAsync()
        {
            return await _penality.GetAllPenalitiesAsync();
        }

        public async Task CreatePenalityAsync(CreatePenalityDTO penalityDTO)
        {
            await _penality.CreatePenalityAsync(penalityDTO);
        }
    }
}