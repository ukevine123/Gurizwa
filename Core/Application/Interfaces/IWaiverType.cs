using Domain.Entities;

namespace Application.Interfaces
{
    public interface IWaiverType
    {
        Task<WaiverType?> GetWaiverTypeByIdAsync(int id);
        Task<IEnumerable<WaiverType>> GetAllWaiverTypesAsync();
        Task<WaiverType?> GetWaiverTypeByNameAndProductAsync(string waiverTypeName, int loanProductId);
        Task<IEnumerable<WaiverType>> GetWaiverTypesByLoanProductAsync(int loanProductId);
        Task<WaiverType> CreateWaiverTypeAsync(WaiverType waiverType);
        Task<WaiverType> UpdateWaiverTypeAsync(WaiverType waiverType);
        Task<bool> DeleteWaiverTypeAsync(int id);
    }
}
