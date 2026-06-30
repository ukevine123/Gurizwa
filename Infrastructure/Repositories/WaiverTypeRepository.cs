using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WaiverTypeRepository : IWaiverType
    {
        private readonly ApplicationDbContext _context;

        public WaiverTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WaiverType?> GetWaiverTypeByIdAsync(int id)
        {
            try
            {
                return await _context.WaiverTypes.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverTypeRepository.GetWaiverTypeById] Error: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<WaiverType>> GetAllWaiverTypesAsync()
        {
            try
            {
                return await _context.WaiverTypes.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverTypeRepository.GetAllWaiverTypes] Error: {ex.Message}");
                return Enumerable.Empty<WaiverType>();
            }
        }

        public async Task<WaiverType?> GetWaiverTypeByNameAndProductAsync(string waiverTypeName, int loanProductId)
        {
            try
            {
                return await _context.WaiverTypes.AsNoTracking()
                    .FirstOrDefaultAsync(w => w.WaiverTypeName == waiverTypeName && w.LoanProductId == loanProductId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverTypeRepository.GetWaiverTypeByNameAndProduct] Error: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<WaiverType>> GetWaiverTypesByLoanProductAsync(int loanProductId)
        {
            try
            {
                return await _context.WaiverTypes.AsNoTracking()
                    .Where(w => w.LoanProductId == loanProductId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverTypeRepository.GetWaiverTypesByLoanProduct] Error: {ex.Message}");
                return Enumerable.Empty<WaiverType>();
            }
        }

        public async Task<WaiverType> CreateWaiverTypeAsync(WaiverType waiverType)
        {
            try
            {
                _context.WaiverTypes.Add(waiverType);
                await _context.SaveChangesAsync();
                return waiverType;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverTypeRepository.CreateWaiverType] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<WaiverType> UpdateWaiverTypeAsync(WaiverType waiverType)
        {
            try
            {
                _context.WaiverTypes.Update(waiverType);
                await _context.SaveChangesAsync();
                return waiverType;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverTypeRepository.UpdateWaiverType] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteWaiverTypeAsync(int id)
        {
            try
            {
                var waiverType = await _context.WaiverTypes.FirstOrDefaultAsync(w => w.Id == id);
                if (waiverType == null)
                    return false;

                _context.WaiverTypes.Remove(waiverType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WaiverTypeRepository.DeleteWaiverType] Error: {ex.Message}");
                return false;
            }
        }
    }
}
