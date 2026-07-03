using Application.DTO;

using Application.Interfaces;

namespace Application.Interfaces
{
    public interface IIdentity
    {
         Task RegisterUser (RegisterUserDTO dto);
         Task<List<UserDetailDTO>> GetAllUsers();
            Task<UserDetailDTO> GetUserById(int id);
            Task UpdateUser(int id, UserDetailDTO dto);
            Task<bool> LoginAsync(LoginDTO dto);
            Task LogoutAsync();
            
            Task RegisterSubUser(RegisterUserDTO dto, int parentPersonId);
            Task<List<UserDetailDTO>> GetSubUsers(int parentPersonId);
            
            Task<List<string>> GetRolesAsync(int parentPersonId);
            Task CreateRoleAsync(string roleName, int parentPersonId);

             /// <summary>
        /// Gets the current authenticated user's profile, or null if not authenticated.
        /// </summary>
        // Task<UserDetailDTO?> GetCurrentUserAsync();
    }
    }