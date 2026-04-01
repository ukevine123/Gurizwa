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
        
        // Standard payment (Full amount on time)
        Task CreatePaymentAsync(CreatePaymentDTO paymentDTO);

        // NEW: Handles the 5% penalty and balance reload logic
        Task CreatePaymentWithPenaltyAsync(CreatePaymentDTO paymentDTO, decimal shortfall, decimal penaltyAmount);

        // Retrieves the expected $70,000 and the due date from the Amortization Schedule
        Task<(decimal Amount, DateTime Date)> GetNextScheduledPaymentAsync(int disbursementId);
}


}