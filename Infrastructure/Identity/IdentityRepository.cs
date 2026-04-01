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
                var _person = new Person
                    {
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Sex = dto.Sex,
                        Status = "Active",
                        DateOfBirth = DateTime.Now,
                        phoneNumber = dto.PhoneNumber,
                        Email = dto.Email,
                        Country = dto.Country,
                        
                        CreatedBy=dto.Email, 
                        UpdateBy=dto.Email,
                                

                    };
                    _dbContext.Persons.Add(_person);
                    _dbContext.SaveChanges();

                var newUser = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                     PersonId = _person.Id,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = true, // Set to true for immediate access
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                    
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

        public async Task<List<UserDetailDTO>> GetAllUsers()
        {
            try
            {
                Console.WriteLine("[GetAllUsers] Fetching users...");

                var users = await _userManager.Users
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                Console.WriteLine($"[GetAllUsers] Found {users.Count} users");

                return users.Select(u => new UserDetailDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                }).ToList();
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
                
                return new UserDetailDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
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
    }
}