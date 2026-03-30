using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;


namespace  Application.Services.Guarantors
{
    public class GuarantorService : IGuarantorService
    { 
     private readonly IGuarantor _guarantor;
        public GuarantorService(IGuarantor guarantor)
        {
            _guarantor = guarantor;

        }
         public async Task<List<Guarantor>> GetAllGuarantorsAsync()
        {
         return await _guarantor.GetAllGuarantorsAsync();
        }
         public async Task<Guarantor> GetGuarantorById(int Id)
        {
            return await _guarantor.GetGuarantorById(Id);
        }
       public async Task CreateGuarantor(CreateGuarantorDTO guarantorDTO)
        {
            await _guarantor.CreateGuarantor( guarantorDTO);
        }
      public async Task UpdateGuarantor(int Id, UpdateGuarantorDTO guarantorDTO)
        {
            await _guarantor.UpdateGuarantor(Id, guarantorDTO);
        }
    }
}