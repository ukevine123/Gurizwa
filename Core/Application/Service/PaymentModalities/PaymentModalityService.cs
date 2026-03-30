using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;


namespace  Application.Services.PaymentModalities
{
    public class PaymentModalityService : IPaymentModalityService
    { 
     private readonly IPaymentModality _paymentModality;
        public PaymentModalityService(IPaymentModality paymentModality)
        {
            _paymentModality = paymentModality;

        }
         public async Task<List<PaymentModality>> GetAllPaymentModalitysAsync()
        {
         return await _paymentModality.GetAllPaymentModalitysAsync();
        }
         public async Task<PaymentModality> GetPaymentModalityById(int Id)
        {
            return await _paymentModality.GetPaymentModalityById(Id);
        }
       public async Task CreatePaymentModality(CreatePaymentModalityDTO paymentModalityDTO)
        {
            await _paymentModality.CreatePaymentModality( paymentModalityDTO);
        }
    
    }
}