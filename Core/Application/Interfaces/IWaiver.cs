using Domain.Entities;

namespace Application.Interfaces
{
    public interface IWaiver
    {
        Task<Waiver> GetWaiverByIdAsync(int id);
        Task<IEnumerable<Waiver>> GetAllWaiversAsync();
        Task<IEnumerable<Waiver>> GetWaiversByDisbursementIdAsync(int disbursementId);
        Task<IEnumerable<Waiver>> GetPendingWaiversAsync();
        Task<IEnumerable<Waiver>> GetApprovedWaiversAsync();
        Task<Waiver> CreateWaiverAsync(Waiver waiver);
        Task<Waiver> UpdateWaiverAsync(Waiver waiver);
        Task<bool> DeleteWaiverAsync(int id);
        Task<bool> ApproveWaiverAsync(int id, string approvedBy);
        Task<bool> RejectWaiverAsync(int id);
    }
}
