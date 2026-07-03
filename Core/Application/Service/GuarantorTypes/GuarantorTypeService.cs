using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;


namespace  Application.Services.GuarantorTypes
{
    public class GuarantorTypeService : IGuarantorTypeService
    { 
     private readonly IGuarantorType _guarantorType;
        public GuarantorTypeService(IGuarantorType guarantorType)
        {
            _guarantorType = guarantorType;

        }
         public async Task<List<GuarantorType>> GetAllGuarantorTypesAsync()
        {
         return await _guarantorType.GetAllGuarantorTypesAsync();
        }
         public async Task<GuarantorType> GetGuarantorTypeById(int Id)
        {
            return await _guarantorType.GetGuarantorTypeById(Id);
        }
       public async Task CreateGuarantorType(CreateGuarantorTypeDTO guarantorTypeDTO)
        {
            await _guarantorType.CreateGuarantorType( guarantorTypeDTO);
        }
      
    }
}