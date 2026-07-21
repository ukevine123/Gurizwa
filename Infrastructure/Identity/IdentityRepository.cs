using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
// using Domain.ValueObjects;

using Domain.Entities;


namespace Infrastructure.Identity
{
    public class IdentityRepository : IIdentity
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public IdentityRepository(
            ApplicationDbContext context,
            UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            RoleManager<IdentityRole<int>> roleManager)
        {
            _dbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<bool> LoginAsync(LoginDTO dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    Console.WriteLine($"[Login] User not found: {dto.Email}");
                    return false;
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName ?? dto.Email,
                    dto.Password,
                    dto.RememberMe,
                    lockoutOnFailure: true
                );

                if (result.Succeeded && !user.IsApproved)
                {
                    await _signInManager.SignOutAsync();
                    throw new InvalidOperationException("Your account is pending approval by an administrator.");
                }

                Console.WriteLine($"[Login] Result for {dto.Email}: {result.Succeeded}");
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Login] Error: {ex.Message}");
                throw;
            }
        }

        public async Task RegisterUser(RegisterUserDTO dto)
        {
            try
            {
                Console.WriteLine($"[RegisterUser] Starting registration for: {dto.Email}");

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    Console.WriteLine($"[RegisterUser] User already exists: {dto.Email}");
                    throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
                }
                bool isOrg = dto.TenantType == "Business";

                var _person = new Person
                    {
                        // For Business: store CompanyName in FirstName, ContactPerson in LastName
                        FirstName  = isOrg ? dto.CompanyName  : dto.FirstName,
                        LastName   = isOrg ? dto.ContactPerson : dto.LastName,
                        Sex        = isOrg ? "N/A"             : dto.Sex.ToString(),
                        Status     = "Active",
                        DateOfBirth = isOrg ? DateTime.MinValue : (dto.DateOfBirth ?? DateTime.Now),
                        phoneNumber = dto.PhoneNumber,
                        Email       = dto.Email,
                        Country     = isOrg ? string.Empty : (dto.Country ?? string.Empty),

                        TenantType    = dto.TenantType,
                        CompanyName   = dto.CompanyName,
                        TinNumber     = dto.TinNumber,
                        ContactPerson = dto.ContactPerson,
                        
                        CreatedBy = dto.Email, 
                        UpdateBy  = dto.Email,
                    };
                    _dbContext.Persons.Add(_person);
                    _dbContext.SaveChanges();

                var newUser = new User
                {
                    FirstName      = isOrg ? dto.CompanyName  : dto.FirstName,
                    LastName       = isOrg ? dto.ContactPerson : dto.LastName,
                    Email          = dto.Email,
                    UserName       = dto.Email,
                    PersonId       = _person.Id,
                    TenantId       = dto.TenantId,
                    PhoneNumber    = dto.PhoneNumber,
                    EmailConfirmed = true, // Set to true for immediate access
                    CreatedAt      = DateTime.UtcNow,
                    UpdatedAt      = DateTime.UtcNow
                };

                Console.WriteLine($"[RegisterUser] Creating user with UserManager...");

                var result = await _userManager.CreateAsync(newUser, dto.Password);

                if (!result.Succeeded)
                {
                    // Log all errors
                    Console.WriteLine($"[RegisterUser] Registration failed for {dto.Email}");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"[RegisterUser] Error: {error.Code} - {error.Description}");
                    }

                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to register user: {errors}");
                }

                Console.WriteLine($"[RegisterUser] User created successfully with ID: {newUser.Id}");

                // Verify the user was saved
                var verifyUser = await _userManager.FindByEmailAsync(dto.Email);
                if (verifyUser == null)
                {
                    Console.WriteLine($"[RegisterUser] WARNING: User was created but cannot be found!");
                    throw new InvalidOperationException("User registration verification failed.");
                }

                Console.WriteLine($"[RegisterUser] Registration completed and verified for: {dto.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterUser] Exception: {ex.Message}");
                Console.WriteLine($"[RegisterUser] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task RegisterSubUser(RegisterUserDTO dto, int parentPersonId)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
                }

                // Find the parent user to link this sub-user
                var parentUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonId == parentPersonId && u.ParentUserId == null);

                var newUser = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    PersonId = parentPersonId,
                    ParentUserId = parentUser?.Id,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(newUser, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to register sub-user: {errors}");
                }

                // Ensure roles exist and assign them
                var defaultRoles = new[] { "Staff", "Manager", "Viewer" };
                var rolesToAssign = string.IsNullOrWhiteSpace(dto.Role)
                    ? new[] { "Staff" }
                    : dto.Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var role in rolesToAssign)
                {
                    var internalRoleName = defaultRoles.Contains(role) ? role : $"{parentPersonId}_{role}";

                    if (!await _roleManager.RoleExistsAsync(internalRoleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>(internalRoleName));
                    }
                    await _userManager.AddToRoleAsync(newUser, internalRoleName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterSubUser] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UserDetailDTO>> GetSubUsers(int parentPersonId)
        {
            var users = await _userManager.Users
                .Where(u => u.PersonId == parentPersonId)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            var result = new List<UserDetailDTO>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var cleanRoles = roles.Select(r => r.Contains("_") ? r.Substring(r.IndexOf("_") + 1) : r).ToList();
                var rolesString = string.Join(", ", cleanRoles);

                result.Add(new UserDetailDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    EmailConfirmed = u.EmailConfirmed,
                    IsApproved = u.IsApproved,
                    Role = rolesString,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    ParentUserId = u.ParentUserId
                });
            }
            return result;
        }

        public async Task<List<string>> GetRolesAsync(int parentPersonId)
        {
            var prefix = $"{parentPersonId}_";
            var dbRoles = await _roleManager.Roles
                .Where(r => r.Name != "Tenant") // Hide Tenant role
                .Select(r => r.Name)
                .ToListAsync();
            
            var resultRoles = new List<string> { "Staff", "Manager", "Viewer" };
            
            foreach (var r in dbRoles)
            {
                if (r != null && r.StartsWith(prefix))
                {
                    resultRoles.Add(r.Substring(prefix.Length));
                }
            }
            
            return resultRoles.Distinct().OrderBy(r => r).ToList();
        }

        public async Task CreateRoleAsync(string roleName, int parentPersonId)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty.");

            var internalRoleName = $"{parentPersonId}_{roleName}";

            if (!await _roleManager.RoleExistsAsync(internalRoleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole<int>(internalRoleName));
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role: {errors}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Role '{roleName}' already exists.");
            }
        }

        public async Task<List<UserDetailDTO>> GetAllUsers()
        {
            try
            {
                Console.WriteLine("[GetAllUsers] Fetching users...");

                var users = await _userManager.Users
                    .Where(u => u.ParentUserId == null)
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                Console.WriteLine($"[GetAllUsers] Found {users.Count} top-level users");

                var result = new List<UserDetailDTO>();
                foreach (var u in users)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    var primaryRole = roles.FirstOrDefault() ?? "Tenant";
                    // If it's a prefixed role (like "1_Staff"), clean it for display
                    if (primaryRole.Contains("_"))
                        primaryRole = primaryRole.Substring(primaryRole.IndexOf("_") + 1);

                    result.Add(new UserDetailDTO
                    {
                        Id = u.Id,
                        FirstName = u.FirstName ?? "",
                        LastName = u.LastName ?? "",
                        Email = u.Email ?? "",
                        PhoneNumber = u.PhoneNumber,
                        EmailConfirmed = u.EmailConfirmed,
                        IsApproved = u.IsApproved,
                        Role = primaryRole,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllUsers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDetailDTO> GetUserById(int id)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    Console.WriteLine($"[GetUserById] User not found: {id}");
                    return null;
                }
                
                var roles = await _userManager.GetRolesAsync(user);
                var cleanRoles = roles.Select(r => r.Contains("_") ? r.Substring(r.IndexOf("_") + 1) : r).ToList();
                var rolesString = string.Join(",", cleanRoles);

                return new UserDetailDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    IsApproved = user.IsApproved,
                    Role = rolesString,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetUserById] Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUser(int id, UserDetailDTO dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                
                if (user == null)
                {
                    Console.WriteLine($"[UpdateUser] User not found: {id}");
                    throw new InvalidOperationException($"User with ID {id} not found.");
                }

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.PhoneNumber = dto.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);
                
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"[UpdateUser] Update failed: {errors}");
                    throw new InvalidOperationException($"Failed to update user: {errors}");
                }

                // Update user roles dynamically
                var currentRoles = await _userManager.GetRolesAsync(user);
                var defaultRoles = new[] { "Staff", "Manager", "Viewer" };
                var parentPersonId = user.PersonId;

                var newRoles = string.IsNullOrWhiteSpace(dto.Role)
                    ? Array.Empty<string>()
                    : dto.Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var newInternalRoleNames = newRoles.Select(role => defaultRoles.Contains(role) ? role : $"{parentPersonId}_{role}").ToList();

                var rolesToAdd = newInternalRoleNames.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(newInternalRoleNames).ToList();

                foreach (var role in rolesToAdd)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>(role));
                    }
                    await _userManager.AddToRoleAsync(user, role);
                }

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                Console.WriteLine($"[UpdateUser] User updated successfully: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateUser] Error: {ex.Message}");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                Console.WriteLine("[Logout] User logged out successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logout] Error: {ex.Message}");
                throw;
            }
        }

        public async Task ApproveUserAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    throw new InvalidOperationException("User not found.");

                user.IsApproved = true;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to approve user: {errors}");
                }

                Console.WriteLine($"[ApproveUser] User {userId} approved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApproveUser] Error: {ex.Message}");
                throw;
            }
        }

        // ═══════════════════════════════════════════════
        //  TENANT ACCOUNT SETUP
        // ═══════════════════════════════════════════════

        public async Task SetupTenantUserAsync(SetupTenantUserDTO dto)
        {
            try
            {
                Console.WriteLine($"[SetupTenantUser] Setting up account for: {dto.Email}");

                var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Email == dto.Email);
                if (tenant == null)
                {
                    throw new InvalidOperationException($"No tenant found with email '{dto.Email}'.");
                }

                if (!tenant.IsApproved)
                {
                    throw new InvalidOperationException("Your tenant account is not yet approved.");
                }

                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"An account for '{dto.Email}' has already been set up.");
                }

                bool isOrg = tenant.TenantType == "Business";

                var person = new Person
                {
                    FirstName = isOrg ? tenant.CompanyName : tenant.FirstName,
                    LastName = isOrg ? "Admin" : tenant.LastName, // Or some default
                    Sex = isOrg ? "N/A" : "Unknown",
                    Status = "Active",
                    DateOfBirth = DateTime.Now,
                    phoneNumber = tenant.PhoneNumber,
                    Email = tenant.Email,
                    Country = tenant.Location ?? string.Empty,

                    TenantType = tenant.TenantType,
                    CompanyName = tenant.CompanyName,
                    
                    CreatedBy = dto.Email,
                    UpdateBy = dto.Email,
                };
                
                _dbContext.Persons.Add(person);
                await _dbContext.SaveChangesAsync();

                var newUser = new User
                {
                    FirstName = isOrg ? tenant.CompanyName : tenant.FirstName,
                    LastName = isOrg ? "Admin" : tenant.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    PersonId = person.Id,
                    TenantId = tenant.Id,
                    PhoneNumber = tenant.PhoneNumber,
                    EmailConfirmed = true,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(newUser, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to setup account: {errors}");
                }

                // Create and assign "Tenant" role if it doesn't exist
                if (!await _roleManager.RoleExistsAsync("Tenant"))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>("Tenant"));
                }
                await _userManager.AddToRoleAsync(newUser, "Tenant");

                Console.WriteLine($"[SetupTenantUser] Account successfully set up for: {dto.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetupTenantUser] Error: {ex.Message}");
                throw;
            }
        }

        // ═══════════════════════════════════════════════
        //  SUB-USER (AGENT) MANAGEMENT
        // ═══════════════════════════════════════════════

        public async Task CreateSubUserAsync(CreateSubUserDTO dto, int parentUserId)
        {
            try
            {
                Console.WriteLine($"[CreateSubUser] Creating agent {dto.Email} for manager {parentUserId}");

                var existing = await _userManager.FindByEmailAsync(dto.Email);
                if (existing != null)
                    throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

                // Create Person record (same pattern as RegisterUser)
                var person = new Person
                {
                    FirstName   = dto.FirstName,
                    LastName    = dto.LastName,
                    Email       = dto.Email,
                    phoneNumber = dto.PhoneNumber,
                    Status      = "Active",
                    Sex         = "Unknown",
                    DateOfBirth = DateTime.Now,
                    Country     = string.Empty,
                    CreatedBy   = dto.Email,
                    UpdateBy    = dto.Email,
                };
                _dbContext.Persons.Add(person);
                await _dbContext.SaveChangesAsync();

                // Create Identity user linked to the parent Manager
                var user = new User
                {
                    FirstName      = dto.FirstName,
                    LastName       = dto.LastName,
                    Email          = dto.Email,
                    UserName       = dto.Email,
                    PhoneNumber    = dto.PhoneNumber,
                    PersonId       = person.Id,
                    ParentUserId   = parentUserId,
                    EmailConfirmed = true,
                    IsApproved     = true,
                    CreatedAt      = DateTime.UtcNow,
                    UpdatedAt      = DateTime.UtcNow,
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create agent: {errors}");
                }

                // Assign Agent role (create if it doesn't exist)
                if (!await _roleManager.RoleExistsAsync("Agent"))
                    await _roleManager.CreateAsync(new IdentityRole<int>("Agent"));

                await _userManager.AddToRoleAsync(user, "Agent");
                Console.WriteLine($"[CreateSubUser] Agent created: {user.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateSubUser] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UserDetailDTO>> GetSubUsersAsync(int parentUserId)
        {
            try
            {
                var subUsers = await _userManager.Users
                    .Where(u => u.ParentUserId == parentUserId)
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                var result = new List<UserDetailDTO>();
                foreach (var u in subUsers)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    var primaryRole = roles.FirstOrDefault() ?? "Agent";
                    if (primaryRole.Contains("_"))
                        primaryRole = primaryRole.Substring(primaryRole.IndexOf("_") + 1);

                    result.Add(new UserDetailDTO
                    {
                        Id             = u.Id,
                        FirstName      = u.FirstName  ?? "",
                        LastName       = u.LastName   ?? "",
                        Email          = u.Email      ?? "",
                        PhoneNumber    = u.PhoneNumber,
                        EmailConfirmed = u.EmailConfirmed,
                        IsApproved     = u.IsApproved,
                        Role           = primaryRole,
                        IsActive       = u.EmailConfirmed,
                        CreatedAt      = u.CreatedAt,
                        UpdatedAt      = u.UpdatedAt,
                        ParentUserId   = u.ParentUserId
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetSubUsers] Error: {ex.Message}");
                throw;
            }
        }

        public async Task SetSubUserStatusAsync(int subUserId, bool isActive, int parentUserId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(subUserId.ToString());
                if (user == null || user.ParentUserId != parentUserId)
                    throw new InvalidOperationException("Agent not found or access denied.");

                user.EmailConfirmed = isActive;
                user.UpdatedAt      = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                Console.WriteLine($"[SetSubUserStatus] Agent {subUserId} set to active={isActive}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetSubUserStatus] Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteSubUserAsync(int subUserId, int parentUserId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(subUserId.ToString());
                if (user == null || user.ParentUserId != parentUserId)
                    throw new InvalidOperationException("Agent not found or access denied.");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to delete agent: {errors}");
                }
                Console.WriteLine($"[DeleteSubUser] Agent {subUserId} deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteSubUser] Error: {ex.Message}");
                throw;
            }
        }
    }
}