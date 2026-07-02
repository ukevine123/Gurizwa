using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Application.Services.Payments;

namespace Application.Services.Payments
{
   public interface IPaymentService
{
   Task<List<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        
        // Standard payment (Early, Full, or partial BEFORE deadline)
        Task CreatePaymentAsync(CreatePaymentDTO paymentDTO);

        // This is the missing method the compiler is looking for:
        Task CreatePaymentWithPenaltyAsync(CreatePaymentDTO paymentDTO, decimal shortfall, decimal penaltyAmount);

        // Retrieves the expected amount and the due date
        Task<(decimal Amount, DateTime Date)> GetNextScheduledPaymentAsync(int disbursementId);
    }
}