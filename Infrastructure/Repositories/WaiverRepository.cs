using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WaiverRepository : IWaiver
    {
        private readonly ApplicationDbContext _context;

        public WaiverRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Waiver> GetWaiverByIdAsync(int id)
        {
            try
            {
                return await _context.Waivers
                    .Include(w => w.Disbursement)
                    .FirstOrDefaultAsync(w => w.Id == id && w.IsActive);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetWaiverById] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Waiver>> GetAllWaiversAsync()
        {
            try
            {
                return await _context.Waivers
                    .Include(w => w.Disbursement)
                    .Where(w => w.IsActive)
                    .OrderByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllWaivers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Waiver>> GetWaiversByDisbursementIdAsync(int disbursementId)
        {
            try
            {
                return await _context.Waivers
                    .Include(w => w.Disbursement)
                    .Where(w => w.DisbursementId == disbursementId && w.IsActive)
                    .OrderByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetWaiversByDisbursementId] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Waiver>> GetPendingWaiversAsync()
        {
            try
            {
                return await _context.Waivers
                    .Include(w => w.Disbursement)
                    .Where(w => w.Status == "Pending" && w.IsActive)
                    .OrderByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetPendingWaivers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Waiver>> GetApprovedWaiversAsync()
        {
            try
            {
                return await _context.Waivers
                    .Include(w => w.Disbursement)
                    .Where(w => w.Status == "Approved" && w.IsActive)
                    .OrderByDescending(w => w.ApprovedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetApprovedWaivers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Waiver> CreateWaiverAsync(Waiver waiver)
        {
            try
            {
                waiver.CreatedAt = DateTime.Now;
                waiver.UpdatedAt = DateTime.Now;
                _context.Waivers.Add(waiver);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[CreateWaiver] Waiver created successfully with ID: {waiver.Id}");
                return waiver;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Waiver> UpdateWaiverAsync(Waiver waiver)
        {
            try
            {
                waiver.UpdatedAt = DateTime.Now;
                _context.Waivers.Update(waiver);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[UpdateWaiver] Waiver updated successfully with ID: {waiver.Id}");
                return waiver;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteWaiverAsync(int id)
        {
            try
            {
                var waiver = await GetWaiverByIdAsync(id);
                if (waiver == null)
                {
                    Console.WriteLine($"[DeleteWaiver] Waiver not found: {id}");
                    return false;
                }

                waiver.IsActive = false;
                await UpdateWaiverAsync(waiver);
                Console.WriteLine($"[DeleteWaiver] Waiver soft-deleted: {id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ApproveWaiverAsync(int id, string approvedBy)
        {
            try
            {
                var waiver = await GetWaiverByIdAsync(id);
                if (waiver == null)
                {
                    Console.WriteLine($"[ApproveWaiver] Waiver not found: {id}");
                    return false;
                }

                if (waiver.Disbursement != null)
                {
                    var disbursement = await _context.Disbursements.FindAsync(waiver.DisbursementId);
                    if (disbursement != null)
                    {
                        if (waiver.Component == "Principal")
                        {
                            // Principal waivers reduce the remaining loan balance, not the original disbursed principal amount.
                            disbursement.Amount = Math.Max(0, disbursement.Amount - waiver.Amount);
                        }
                        else if (waiver.Component == "Interest")
                        {
                            disbursement.Amount = Math.Max(0, disbursement.Amount - waiver.Amount);
                        }
                        else if (waiver.Component == "Penalty")
                        {
                            var penalty = await _context.Penalties
                                .Where(p => p.LoanApplicationId == waiver.Disbursement.LoanApplicationId && p.IsActive)
                                .OrderByDescending(p => p.Date)
                                .FirstOrDefaultAsync();

                            if (penalty != null)
                            {
                                penalty.Amount = Math.Max(0, penalty.Amount - waiver.Amount);
                                _context.Penalties.Update(penalty);
                            }

                            disbursement.Amount = Math.Max(0, disbursement.Amount - waiver.Amount);
                        }

                        _context.Disbursements.Update(disbursement);
                    }
                }

                waiver.Status = "Approved";
                waiver.ApprovedBy = approvedBy;
                waiver.ApprovedDate = DateTime.Now;
                await UpdateWaiverAsync(waiver);
                Console.WriteLine($"[ApproveWaiver] Waiver approved: {id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApproveWaiver] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RejectWaiverAsync(int id)
        {
            try
            {
                var waiver = await GetWaiverByIdAsync(id);
                if (waiver == null)
                {
                    Console.WriteLine($"[RejectWaiver] Waiver not found: {id}");
                    return false;
                }

                waiver.Status = "Rejected";
                waiver.ApprovedDate = DateTime.Now;
                await UpdateWaiverAsync(waiver);
                Console.WriteLine($"[RejectWaiver] Waiver rejected: {id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RejectWaiver] Error: {ex.Message}");
                throw;
            }
        }
    }
}
