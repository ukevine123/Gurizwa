using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Tenants
{
    public class TenantService : ITenantService
    {
        private readonly ITenant _tenantRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public TenantService(ITenant tenantRepository, IEmailService emailService, IConfiguration configuration)
        {
            _tenantRepository = tenantRepository;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<bool> RegisterTenantAsync(TenantRegistrationDTO dto)
        {
            try
            {
                var existingTenant = await _tenantRepository.GetTenantByEmailAsync(dto.Email);
                if (existingTenant != null)
                {
                    throw new InvalidOperationException($"A tenant with email '{dto.Email}' already exists.");
                }

                var tenant = new Tenant
                {
                    TenantType = dto.TenantType,
                    CompanyName = dto.CompanyName,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    Location = dto.Location,
                    Status = "Pending",
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                foreach (var doc in dto.Documents)
                {
                    tenant.TenantDocuments.Add(new TenantDocument
                    {
                        DocumentName = doc.FileName,
                        DocumentFile = doc.Content,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _tenantRepository.AddTenantAsync(tenant);

                // Send email to tenant
                string subject = "Registration Received - Pending Approval";
                string body = $"<h3>Hello,</h3><p>Your registration for Guriza has been received. Please wait for an administrator to approve your account.</p>";
                await _emailService.SendEmailAsync(tenant.Email, subject, body);

                // Send email to admin
                string appUrl = _configuration["AppUrl"] ?? "https://localhost:7082";
                string adminEmail = _configuration["AdminEmail"] ?? "admin@example.com";
                string adminSubject = "New Tenant Registration Requires Approval";
                string adminBody = $"<h3>Hello Admin,</h3><p>A new tenant ({tenant.CompanyName ?? tenant.FirstName + " " + tenant.LastName}) has registered and is awaiting approval.</p><p>Please log in to the admin portal and navigate to <a href='{appUrl}/tenants'>Tenants</a> to review and approve their registration.</p>";
                await _emailService.SendEmailAsync(adminEmail, adminSubject, adminBody);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TenantService] Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ApproveTenantAsync(int tenantId)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId);
            if (tenant == null) return false;

            tenant.IsApproved = true;
            tenant.Status = "Approved";
            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepository.UpdateTenantAsync(tenant);

            string appUrl = _configuration["AppUrl"] ?? "https://localhost:7082";
            string subject = "Account Approved - Create Your Password";
            string body = $"<h3>Hello,</h3><p>Your tenant registration has been approved.</p><p>Please <a href='{appUrl}/account/setup?email={tenant.Email}'>click here</a> to create your password and finalize your account setup.</p>";
            await _emailService.SendEmailAsync(tenant.Email, subject, body);

            return true;
        }

        public async Task<bool> RejectTenantAsync(int tenantId)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId);
            if (tenant == null) return false;

            tenant.Status = "Rejected";
            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepository.UpdateTenantAsync(tenant);
            return true;
        }

        public async Task<List<TenantDTO>> GetPendingTenantsAsync()
        {
            var tenants = await _tenantRepository.GetPendingTenantsAsync();
            return tenants.Select(MapToDTO).ToList();
        }

        public async Task<List<TenantDTO>> GetAllTenantsAsync()
        {
            var tenants = await _tenantRepository.GetAllTenantsAsync();
            return tenants.Select(MapToDTO).ToList();
        }

        public async Task<TenantDTO> GetTenantByEmailAsync(string email)
        {
            var tenant = await _tenantRepository.GetTenantByEmailAsync(email);
            return tenant != null ? MapToDTO(tenant) : null;
        }

        public async Task<TenantDTO> GetTenantByIdAsync(int id)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(id);
            return tenant != null ? MapToDTO(tenant) : null;
        }

        private TenantDTO MapToDTO(Tenant tenant)
        {
            return new TenantDTO
            {
                Id = tenant.Id,
                TenantType = tenant.TenantType,
                CompanyName = tenant.CompanyName,
                FirstName = tenant.FirstName,
                LastName = tenant.LastName,
                PhoneNumber = tenant.PhoneNumber,
                Email = tenant.Email,
                Location = tenant.Location,
                IsApproved = tenant.IsApproved,
                Status = tenant.Status,
                CreatedAt = tenant.CreatedAt,
                Documents = tenant.TenantDocuments.Select(d => new TenantDocumentDTO
                {
                    Id = d.Id,
                    DocumentName = d.DocumentName,
                    DocumentFile = d.DocumentFile
                }).ToList()
            };
        }
    }
}
