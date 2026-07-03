using Domain.Entities;
using Application.DTO;
using Application.Interfaces;

namespace Application.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPayment _payment;

        public PaymentService(IPayment payment)
        {
            _payment = payment;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id) => await _payment.GetPaymentByIdAsync(id);

        public async Task<List<Payment>> GetAllPaymentsAsync() => await _payment.GetAllPaymentsAsync();

        public async Task CreatePaymentAsync(CreatePaymentDTO paymentDTO)
        {
            // 1. Fetch current installment status
            var (expectedAmount, dueDate) = await _payment.GetNextScheduledPaymentAsync(paymentDTO.DisbursementId);

            DateTime today = DateTime.Today; 
            bool isAfterDueDate = today > dueDate;
            
            // 2. Calculate the Shortfall
            decimal shortfall = expectedAmount - paymentDTO.Amount;

            // 3. Execution Logic with 2% Penalty
            if (isAfterDueDate && shortfall > 0)
            {
                // SCENARIO: Payment is late and doesn't cover the scheduled amount.
                // Updated to 2% as requested
                decimal penaltyAmount = shortfall * 0.02m; 

                await _payment.CreatePaymentWithPenaltyAsync(paymentDTO, shortfall, penaltyAmount);
            }
            else
            {
                // SCENARIO: On-time payment, overpayment, or clearing the balance.
                await _payment.CreatePaymentAsync(paymentDTO);
            }
        }

        public async Task CreatePaymentWithPenaltyAsync(CreatePaymentDTO paymentDTO, decimal shortfall, decimal penaltyAmount)
        {
            await _payment.CreatePaymentWithPenaltyAsync(paymentDTO, shortfall, penaltyAmount);
        }

        public async Task<(decimal Amount, DateTime Date)> GetNextScheduledPaymentAsync(int disbursementId)
        {
            return await _payment.GetNextScheduledPaymentAsync(disbursementId);
        }
    }
}