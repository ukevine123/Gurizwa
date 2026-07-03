using Domain.Entities;
using Application.DTO;

namespace Application.Services.PaymentTypes
{
    public interface IPaymentTypeService
    {
        Task<PaymentType?> GetPaymentTypeByIdAsync(int id);

        Task <List<PaymentType>> GetAllPaymentTypesAsync();
        Task CreatePaymentTypeAsync(CreatePaymentTypeDTO paymentTypeDTO);

    }
}