using Domain.Entities;
using Application.DTO;

namespace Application.Services.PaymentModalities
{
    public interface IPaymentModalityService
    {
        Task<List<PaymentModality>> GetAllPaymentModalitysAsync();
        Task<PaymentModality> GetPaymentModalityById(int id);   
        Task CreatePaymentModality(CreatePaymentModalityDTO paymentModalityDTO);
        
    }
}