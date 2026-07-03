using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IGuarantorType
    {
        Task<List<GuarantorType>> GetAllGuarantorTypesAsync();
        Task<GuarantorType> GetGuarantorTypeById(int id);   
        Task CreateGuarantorType(CreateGuarantorTypeDTO guarantorTypeDTO);
        
    }
}
