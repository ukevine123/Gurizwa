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

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _payment.GetPaymentByIdAsync(id);
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _payment.GetAllPaymentsAsync();
        }

        public async Task CreatePaymentAsync(CreatePaymentDTO paymentDTO)
        {
            // 1. Fetch the expected schedule for this disbursement
            var (expectedAmount, dueDate) = await _payment.GetNextScheduledPaymentAsync(paymentDTO.DisbursementId);

            // 2. Map nullable PaymentDate to a non-nullable local variable for comparison
            DateTime actualPaymentDate = paymentDTO.PaymentDate ?? DateTime.Today;

            // 3. Logic Check: Compare using the correct DTO property name
            if (paymentDTO.Amount < expectedAmount || actualPaymentDate > dueDate)
            {
                // Calculate the unpaid gap (Shortfall)
                decimal shortfall = Math.Max(0, expectedAmount - paymentDTO.Amount);
                
                // Calculate the 5% penalty fee on the shortfall
                decimal penaltyAmount = shortfall * 0.05m;

                // 4. Process with penalty logic
                await _payment.CreatePaymentWithPenaltyAsync(paymentDTO, shortfall, penaltyAmount);
            }
            else
            {
                // Standard flow for perfect payments
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