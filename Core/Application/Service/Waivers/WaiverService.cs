using Application.DTO;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services.Waivers
{
    public interface IWaiverService
    {
        Task<WaiverDTO?> GetWaiverByIdAsync(int id);
        Task<IEnumerable<WaiverDTO>> GetAllWaiversAsync();
        Task<IEnumerable<WaiverDTO>> GetWaiversByDisbursementIdAsync(int disbursementId);
        Task<IEnumerable<WaiverDTO>> GetPendingWaiversAsync();
        Task<IEnumerable<WaiverDTO>> GetApprovedWaiversAsync();
        Task<WaiverDTO> CreateWaiverAsync(CreateWaiverDTO createWaiverDto);
        Task<WaiverDTO> UpdateWaiverAsync(WaiverDTO waiverDto);
        Task<bool> ApproveWaiverAsync(int waiverIdDTO, string approvedBy);
        Task<bool> RejectWaiverAsync(int waiverId);
        Task<bool> DeleteWaiverAsync(int id);
        Task<decimal> GetAutoFillAmountAsync(int disbursementId, string waiverType);
    }

    public class WaiverService : IWaiverService
    {
        private readonly IWaiver _waiverRepository;
        private readonly IPenality _penalityRepository;
        private readonly IDisbursement _disbursementRepository;
        private readonly IPayment _paymentRepository;
        private readonly ILoanApplication _loanApplicationRepository;
        private readonly IWaiverType _waiverTypeRepository;

        public WaiverService(IWaiver waiverRepository, IPenality penalityRepository, IDisbursement disbursementRepository, IPayment paymentRepository, ILoanApplication loanApplicationRepository, IWaiverType waiverTypeRepository)
        {
            _waiverRepository = waiverRepository;
            _penalityRepository = penalityRepository;
            _disbursementRepository = disbursementRepository;
            _paymentRepository = paymentRepository;
            _loanApplicationRepository = loanApplicationRepository;
            _waiverTypeRepository = waiverTypeRepository;
        }

        public async Task<WaiverDTO> GetWaiverByIdAsync(int id)
        {
            try
            {
                var waiver = await _waiverRepository.GetWaiverByIdAsync(id);
                return waiver != null ? MapToDto(waiver) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetWaiverById] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<WaiverDTO>> GetAllWaiversAsync()
        {
            try
            {
                var waivers = await _waiverRepository.GetAllWaiversAsync();
                return waivers.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetAllWaivers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<WaiverDTO>> GetWaiversByDisbursementIdAsync(int disbursementId)
        {
            try
            {
                var waivers = await _waiverRepository.GetWaiversByDisbursementIdAsync(disbursementId);
                return waivers.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetWaiversByDisbursementId] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<WaiverDTO>> GetPendingWaiversAsync()
        {
            try
            {
                var waivers = await _waiverRepository.GetPendingWaiversAsync();
                return waivers.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetPendingWaivers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<WaiverDTO>> GetApprovedWaiversAsync()
        {
            try
            {
                var waivers = await _waiverRepository.GetApprovedWaiversAsync();
                return waivers.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetApprovedWaivers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<WaiverDTO> CreateWaiverAsync(CreateWaiverDTO createWaiverDto)
        {
            try
            {
                if (createWaiverDto == null)
                    throw new ArgumentNullException(nameof(createWaiverDto));

                ValidateWaiverType(createWaiverDto.WaiverType);

                // Get the disbursement to find the loan product
                var disbursement = await _disbursementRepository.GetDisbursementByIdAsync(createWaiverDto.DisbursementId);
                if (disbursement == null)
                    throw new InvalidOperationException($"Disbursement with ID {createWaiverDto.DisbursementId} not found");

                // Get the loan application to find the loan product
                var loanApplication = await _loanApplicationRepository.GetLoanApplicationById(disbursement.LoanApplicationId);
                if (loanApplication == null)
                    throw new InvalidOperationException($"Loan application not found");

                // Get the WaiverType with outstanding amount for this loan product
                var waiverType = await _waiverTypeRepository.GetWaiverTypeByNameAndProductAsync(createWaiverDto.WaiverType, loanApplication.LoanProductSetting.LoanProductId);
                if (waiverType == null)
                {
                    // Dynamically create default WaiverType for this product
                    var newWaiverType = new WaiverType
                    {
                        WaiverTypeName = createWaiverDto.WaiverType,
                        LoanProductId = loanApplication.LoanProductSetting.LoanProductId,
                        OutstandingAmount = 0,
                        Description = $"Auto-configured default {createWaiverDto.WaiverType}",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    waiverType = await _waiverTypeRepository.CreateWaiverTypeAsync(newWaiverType);
                }

                var waiver = new Waiver
                {
                    DisbursementId = createWaiverDto.DisbursementId,
                    WaiverTypeId = waiverType.Id,
                    WaiverTypeName = createWaiverDto.WaiverType,
                    Component = NormalizeWaiverComponent(createWaiverDto.WaiverType),
                    Amount = createWaiverDto.Amount ?? waiverType.OutstandingAmount, // Use the submitted amount if provided, fallback to configured
                    Reason = createWaiverDto.Reason,
                    Description = createWaiverDto.Description,
                    Status = "Pending",
                    IsActive = true
                };

                var createdWaiver = await _waiverRepository.CreateWaiverAsync(waiver);
                Console.WriteLine($"[WaiverService.CreateWaiver] Waiver created with ID: {createdWaiver.Id}, Amount: {waiverType.OutstandingAmount}");
                return MapToDto(createdWaiver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.CreateWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<WaiverDTO> UpdateWaiverAsync(WaiverDTO waiverDto)
        {
            try
            {
                if (waiverDto == null)
                    throw new ArgumentNullException(nameof(waiverDto));

                var existingWaiver = await _waiverRepository.GetWaiverByIdAsync(waiverDto.Id);
                if (existingWaiver == null)
                    throw new InvalidOperationException($"Waiver with ID {waiverDto.Id} not found");

                // Get the disbursement to find the loan product
                var disbursement = await _disbursementRepository.GetDisbursementByIdAsync(existingWaiver.DisbursementId);
                if (disbursement == null)
                    throw new InvalidOperationException($"Disbursement with ID {existingWaiver.DisbursementId} not found");

                // Get the loan application to find the loan product
                var loanApplication = await _loanApplicationRepository.GetLoanApplicationById(disbursement.LoanApplicationId);
                if (loanApplication == null)
                    throw new InvalidOperationException($"Loan application not found");

                // Get the WaiverType with outstanding amount for this loan product
                var waiverType = await _waiverTypeRepository.GetWaiverTypeByNameAndProductAsync(waiverDto.WaiverType, loanApplication.LoanProductSetting.LoanProductId);
                if (waiverType == null)
                {
                    // Dynamically create default WaiverType for this product
                    var newWaiverType = new WaiverType
                    {
                        WaiverTypeName = waiverDto.WaiverType,
                        LoanProductId = loanApplication.LoanProductSetting.LoanProductId,
                        OutstandingAmount = 0,
                        Description = $"Auto-configured default {waiverDto.WaiverType}",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    waiverType = await _waiverTypeRepository.CreateWaiverTypeAsync(newWaiverType);
                }

                existingWaiver.WaiverTypeName = waiverDto.WaiverType;
                existingWaiver.WaiverTypeId = waiverType.Id;
                existingWaiver.Component = NormalizeWaiverComponent(waiverDto.WaiverType);
                existingWaiver.Amount = waiverDto.Amount; // Use the updated amount
                existingWaiver.Reason = waiverDto.Reason;
                existingWaiver.Description = waiverDto.Description;

                var updatedWaiver = await _waiverRepository.UpdateWaiverAsync(existingWaiver);
                Console.WriteLine($"[WaiverService.UpdateWaiver] Waiver updated with ID: {updatedWaiver.Id}, Amount: {waiverType.OutstandingAmount}");
                return MapToDto(updatedWaiver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.UpdateWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ApproveWaiverAsync(int waiverId, string approvedBy)
        {
            try
            {
                return await _waiverRepository.ApproveWaiverAsync(waiverId, approvedBy);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.ApproveWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RejectWaiverAsync(int waiverId)
        {
            try
            {
                return await _waiverRepository.RejectWaiverAsync(waiverId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.RejectWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteWaiverAsync(int id)
        {
            try
            {
                return await _waiverRepository.DeleteWaiverAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.DeleteWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> GetAutoFillAmountAsync(int disbursementId, string waiverType)
        {
            try
            {
                if (disbursementId <= 0)
                    return 0;

                // Get disbursement details
                var disbursement = await _disbursementRepository.GetDisbursementByIdAsync(disbursementId);
                
                if (disbursement == null)
                    return 0;

                // Get loan application details
                var loanApplication = await _loanApplicationRepository.GetLoanApplicationById(disbursement.LoanApplicationId);
                
                if (loanApplication == null)
                    return 0;

                // Calculate remaining amount based on waiver type
                return waiverType switch
                {
                    "Penalty Waiver" => await GetOutstandingPenaltyAmount(loanApplication.Id),
                    "Interest Waiver" => await GetOutstandingInterestAmount(disbursement),
                    "Principal Waiver" => await GetOutstandingPrincipalAmount(disbursement),
                    _ => 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetAutoFillAmount] Error: {ex.Message}");
                return 0;
            }
        }

        private async Task<decimal> GetOutstandingPenaltyAmount(int loanApplicationId)
        {
            try
            {
                var penalties = await _penalityRepository.GetAllPenalitiesAsync();
                var totalPenalty = penalties?
                    .Where(p => p.LoanApplicationId == loanApplicationId && p.IsActive)
                    .Sum(p => p.Amount) ?? 0;
                
                // Penalties are typically not part of regular payments, so return full amount
                return totalPenalty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetOutstandingPenaltyAmount] Error: {ex.Message}");
                return 0;
            }
        }

        private async Task<decimal> GetOutstandingInterestAmount(Disbursement disbursement)
        {
            try
            {
                // Calculate total interest based on principal and interest rate
                decimal totalInterest = disbursement.PrincipalOffered * (disbursement.InterestRate / 100);
                
                // Calculate total payments made towards the loan
                decimal totalPaid = disbursement.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                
                // Calculate outstanding interest proportionally
                decimal outstandingInterest = 0;
                if (disbursement.Amount > 0)
                {
                    outstandingInterest = totalInterest * (1 - (totalPaid / disbursement.Amount));
                }
                else
                {
                    outstandingInterest = Math.Max(0, totalInterest - totalPaid);
                }
                
                return Math.Round(Math.Max(0, outstandingInterest), 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetOutstandingInterestAmount] Error: {ex.Message}");
                return 0;
            }
        }

        private async Task<decimal> GetOutstandingPrincipalAmount(Disbursement disbursement)
        {
            try
            {
                // Total principal to be repaid
                decimal totalPrincipal = disbursement.PrincipalOffered;
                
                // Calculate total payments made towards the loan
                decimal totalPaid = disbursement.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                
                // Calculate outstanding principal proportionally
                decimal outstandingPrincipal = 0;
                if (disbursement.Amount > 0)
                {
                    outstandingPrincipal = totalPrincipal * (1 - (totalPaid / disbursement.Amount));
                }
                else
                {
                    outstandingPrincipal = Math.Max(0, totalPrincipal - totalPaid);
                }
                
                return Math.Round(Math.Max(0, outstandingPrincipal), 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverService.GetOutstandingPrincipalAmount] Error: {ex.Message}");
                return 0;
            }
        }

        private WaiverDTO MapToDto(Waiver waiver)
        {
            return new WaiverDTO
            {
                Id = waiver.Id,
                DisbursementId = waiver.DisbursementId,
                ApplicationCode = waiver.Disbursement?.LoanApplication?.ApplicationCode ?? "N/A",
                WaiverType = waiver.WaiverTypeName,
                Component = waiver.Component,
                Amount = waiver.Amount,
                Reason = waiver.Reason,
                Description = waiver.Description,
                Status = waiver.Status,
                ApprovedBy = waiver.ApprovedBy,
                ApprovedDate = waiver.ApprovedDate,
                IsActive = waiver.IsActive,
                CreatedAt = waiver.CreatedAt,
                UpdatedAt = waiver.UpdatedAt
            };
        }

        private static void ValidateWaiverType(string waiverType)
        {
            if (string.IsNullOrWhiteSpace(waiverType) ||
                (waiverType != "Penalty Waiver" && waiverType != "Interest Waiver" && waiverType != "Principal Waiver"))
            {
                throw new ArgumentException("Waiver type must be Penalty Waiver, Interest Waiver, or Principal Waiver.", nameof(waiverType));
            }
        }

        private static string NormalizeWaiverComponent(string waiverType)
        {
            return waiverType switch
            {
                "Penalty Waiver" => "Penalty",
                "Interest Waiver" => "Interest",
                "Principal Waiver" => "Principal",
                _ => throw new ArgumentException("Invalid waiver type. Must be Penalty Waiver, Interest Waiver, or Principal Waiver.", nameof(waiverType)),
            };
        }
    }
}
