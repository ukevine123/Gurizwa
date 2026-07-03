using Application.DTO;

using Application.Interfaces;

namespace Application.Interfaces
{
    public interface IIdentity
    {
        // ── Existing ──────────────────────────────────────────
        Task RegisterUser(RegisterUserDTO dto);
        Task<List<UserDetailDTO>> GetAllUsers();
        Task<UserDetailDTO> GetUserById(int id);
        Task UpdateUser(int id, UserDetailDTO dto);
        Task<bool> LoginAsync(LoginDTO dto);
        Task LogoutAsync();

        // ── Sub-User (Agent) Management ───────────────────────

        /// <summary>Creates a new Agent account owned by the given Manager.</summary>
        Task CreateSubUserAsync(CreateSubUserDTO dto, int parentUserId);

        /// <summary>Returns all Agents that belong to a specific Manager.</summary>
        Task<List<UserDetailDTO>> GetSubUsersAsync(int parentUserId);

        /// <summary>Activates or deactivates an Agent (Manager must own them).</summary>
        Task SetSubUserStatusAsync(int subUserId, bool isActive, int parentUserId);

        /// <summary>Permanently deletes an Agent (Manager must own them).</summary>
        Task DeleteSubUserAsync(int subUserId, int parentUserId);
    }
}