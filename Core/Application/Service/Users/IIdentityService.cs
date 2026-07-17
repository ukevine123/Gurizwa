using Application.DTO;
using Application.Services.Users;
using Application.Interfaces;   

namespace Application.Services.Users
{
    public class IdentityService : IIdentityService
    {
        private readonly IIdentity _identity;

        public IdentityService(IIdentity identity)
        {
            _identity = identity;
        }
        public async Task<bool> LoginAsync(LoginDTO dto)
         { 
            return await _identity.LoginAsync(dto);
             }
          public async Task LogoutAsync() 
          
          { 
            await _identity.LogoutAsync();
            
            }

        public async Task ApproveUserAsync(int userId)
        {
            await _identity.ApproveUserAsync(userId);
        }

        public async Task RegisterUser(RegisterUserDTO dto)
        {
            await _identity.RegisterUser(dto);
        }
        
        public async Task RegisterSubUser(RegisterUserDTO dto, int parentPersonId)
        {
            await _identity.RegisterSubUser(dto, parentPersonId);
        }
        
        public async Task CreateSubUserAsync(CreateSubUserDTO dto, int parentUserId)
        {
            await _identity.CreateSubUserAsync(dto, parentUserId);
        }
        public async Task<List<UserDetailDTO>> GetSubUsersAsync(int parentUserId)
        {
            return await _identity.GetSubUsersAsync(parentUserId);
        }
        public async Task SetSubUserStatusAsync(int subUserId, bool isActive, int parentUserId)
        {
            await _identity.SetSubUserStatusAsync(subUserId, isActive, parentUserId);
        }
        public async Task DeleteSubUserAsync(int subUserId, int parentUserId)
        {
            await _identity.DeleteSubUserAsync(subUserId, parentUserId);
        }
        public async Task<List<UserDetailDTO>> GetSubUsers(int parentPersonId)
        {
            return await _identity.GetSubUsers(parentPersonId);
        }

        public async Task<List<string>> GetRolesAsync(int parentPersonId)
        {
            return await _identity.GetRolesAsync(parentPersonId);
        }

        public async Task CreateRoleAsync(string roleName, int parentPersonId)
        {
            await _identity.CreateRoleAsync(roleName, parentPersonId);
        }
        public async Task<List<UserDetailDTO>> GetAllUsers()
        {
            return await _identity.GetAllUsers();
        }
        public async Task<UserDetailDTO> GetUserById(int id)
        {
            return await _identity.GetUserById(id);
        }
        public async Task UpdateUser(int id, UserDetailDTO dto)
        {
            await _identity.UpdateUser(id, dto);
        }

        public async Task SetupTenantUserAsync(SetupTenantUserDTO dto)
        {
            await _identity.SetupTenantUserAsync(dto);
        }

    }
}
