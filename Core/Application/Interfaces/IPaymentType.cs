using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IPaymentType
    {
        Task<PaymentType?> GetPaymentTypeByIdAsync(int id);

        Task <List<PaymentType>> GetAllPaymentTypesAsync();
        Task CreatePaymentTypeAsync(CreatePaymentTypeDTO paymentTypeDTO);
    }
}