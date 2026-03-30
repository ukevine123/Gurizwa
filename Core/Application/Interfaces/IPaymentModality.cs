using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IPaymentModality
    {
        // Task<List<PaymentModality>> GetAllPaymentModalityAsync();
        Task<PaymentModality> GetPaymentModalityById(int id);   
        Task CreatePaymentModality(CreatePaymentModalityDTO paymentModalityDTO);
        Task<List<PaymentModality>> GetAllPaymentModalitysAsync();
    }
}
