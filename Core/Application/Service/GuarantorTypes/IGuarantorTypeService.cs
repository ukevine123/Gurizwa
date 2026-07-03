using Domain.Entities;
using Application.DTO;

namespace Application.Services.GuarantorTypes
{
    public interface IGuarantorTypeService
    {
        Task <GuarantorType> GetGuarantorTypeById(int Id);
        Task <List<GuarantorType>> GetAllGuarantorTypesAsync();
        Task CreateGuarantorType(CreateGuarantorTypeDTO guarantorTypeDTO);
       
    }
}