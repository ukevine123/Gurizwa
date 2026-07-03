using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IGuarantor
    {
        Task<List<Guarantor>> GetAllGuarantorsAsync();
        Task<Guarantor> GetGuarantorById(int id);   
        Task CreateGuarantor(CreateGuarantorDTO guarantorDTO);
        Task UpdateGuarantor(int id, UpdateGuarantorDTO guarantorDTO);
    }
}
