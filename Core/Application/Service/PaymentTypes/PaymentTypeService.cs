using Domain.Entities;
using Application.DTO;
using Application.Interfaces;

namespace Application.Services.PaymentTypes
{
    public class PaymentTypeService : IPaymentTypeService
    {

    private readonly IPaymentType _paymentType;

    public PaymentTypeService(IPaymentType paymentType)
    {
        _paymentType = paymentType;
    }
   
       public async Task<PaymentType?> GetPaymentTypeByIdAsync(int id)
    {
        return await _paymentType.GetPaymentTypeByIdAsync(id);
        
    }

     public async Task<List<PaymentType>> GetAllPaymentTypesAsync()
        {
            return await _paymentType.GetAllPaymentTypesAsync();
        }

        public async Task CreatePaymentTypeAsync(CreatePaymentTypeDTO paymentTypeDTO)
        {
             await _paymentType.CreatePaymentTypeAsync(paymentTypeDTO);
        }

     

}
}