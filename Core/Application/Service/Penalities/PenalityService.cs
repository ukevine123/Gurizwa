using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Application.Services.Disbursements;
using Application.Services.Payments;
using Application.Services.Reasons;

namespace Application.Services.Penalities
{
    public class PenalityService : IPenalityService
    {
        private readonly IPenality _penality;
        private readonly IDisbursementService _disbursementService;
        private readonly IPaymentService _paymentService;
        private readonly IReasonService _reasonService;

        public PenalityService(IPenality penality, IDisbursementService disbursementService, IPaymentService paymentService, IReasonService reasonService)
        {
            _penality = penality;
            _disbursementService = disbursementService;
            _paymentService = paymentService;
            _reasonService = reasonService;
        }

        public async Task <Penality?> GetPenalityByIdAsync(int id)
        {
            return await _penality.GetPenalityByIdAsync(id);
        }

        public async Task<List<Penality>> GetAllPenalitiesAsync()
        {
            return await _penality.GetAllPenalitiesAsync();
        }

        public async Task CreatePenalityAsync(CreatePenalityDTO penalityDTO)
        {
            await _penality.CreatePenalityAsync(penalityDTO);
        }

        public async Task GenerateAutomaticPenalitiesAsync()
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();
            var allPayments = await _paymentService.GetAllPaymentsAsync();
            var reasons = await _reasonService.GetAllReasonsAsync();
            var lateReason = reasons.FirstOrDefault(r => r.Name.Contains("Late", StringComparison.OrdinalIgnoreCase) || r.Name.Contains("Overdue", StringComparison.OrdinalIgnoreCase));
            
            if (lateReason == null && reasons.Any())
            {
                lateReason = reasons.First();
            }

            var allPenalties = await _penality.GetAllPenalitiesAsync();

            foreach (var disbursement in disbursements.Where(d => d.IsActive))
            {
                if (disbursement.TotalInstallments <= 0 || disbursement.PrincipalOffered <= 0)
                    continue;

                decimal scheduledPayment = disbursement.Amount / disbursement.TotalInstallments;
                
                var paymentsByInstallment = allPayments.Where(p => p.DisbursementId == disbursement.Id && p.IsActive).ToList();
                decimal totalPaidPool = paymentsByInstallment.Sum(p => p.Amount);

                for (int i = 0; i < disbursement.TotalInstallments; i++)
                {
                    DateTime dueDate = disbursement.PaymentModality?.Mode?.ToLower() switch 
                    {
                        "daily" => disbursement.StartDate.AddDays(i + 1),
                        "weekly" => disbursement.StartDate.AddDays((i + 1) * 7),
                        "monthly" => disbursement.StartDate.AddMonths(i + 1),
                        "yearly" => disbursement.StartDate.AddYears(i + 1),
                        _ => disbursement.StartDate.AddMonths(i + 1)
                    };

                    decimal amountPaidForInstallment = 0;
                    if (totalPaidPool >= scheduledPayment)
                    {
                        amountPaidForInstallment = scheduledPayment;
                        totalPaidPool -= scheduledPayment;
                    }
                    else if (totalPaidPool > 0)
                    {
                        amountPaidForInstallment = totalPaidPool;
                        totalPaidPool = 0;
                    }

                    if (DateTime.Now.Date > dueDate.Date && amountPaidForInstallment < scheduledPayment)
                    {
                        decimal shortfall = scheduledPayment - amountPaidForInstallment;
                        int installmentNo = i + 1;
                        string descriptionTag = $"[Auto-Inst#{installmentNo}]";
                        
                        bool penaltyExists = allPenalties.Any(p => p.LoanApplicationId == disbursement.LoanApplicationId 
                                                               && p.Description != null 
                                                               && p.Description.Contains(descriptionTag));

                        if (!penaltyExists)
                        {
                            var penaltyDTO = new CreatePenalityDTO
                            {
                                LoanApplicationId = disbursement.LoanApplicationId,
                                Amount = shortfall,
                                ReasonId = lateReason?.Id ?? 0,
                                Description = $"{descriptionTag} Automatic Late Penalty."
                            };

                            if (penaltyDTO.ReasonId > 0)
                            {
                                await _penality.CreatePenalityAsync(penaltyDTO);
                            }
                        }
                    }
                }
            }
        }
    }
}