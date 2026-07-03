using Application.DTO;

namespace Application.Services.Users
{
    public interface IIdentityService
    {
         Task RegisterUser (RegisterUserDTO dto);
         Task<List<UserDetailDTO>> GetAllUsers();
            Task<UserDetailDTO> GetUserById(int id);
            Task UpdateUser(int id, UserDetailDTO dto);
            Task<bool> LoginAsync(LoginDTO dto);
            Task LogoutAsync();
            Task RegisterSubUser(RegisterUserDTO dto, int parentPersonId);
            Task<List<UserDetailDTO>> GetSubUsers(int parentPersonId);
    Task CreateSubUserAsync(CreateSubUserDTO dto, int parentUserId);
    Task<List<UserDetailDTO>> GetSubUsersAsync(int parentUserId);
    Task SetSubUserStatusAsync(int subUserId, bool isActive, int parentUserId);
    Task DeleteSubUserAsync(int subUserId, int parentUserId);
            Task<List<string>> GetRolesAsync(int parentPersonId);
            Task CreateRoleAsync(string roleName, int parentPersonId);
   
    }   
}
