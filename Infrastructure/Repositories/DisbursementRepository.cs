using Domain.Entities;

using Application.DTO;

using Application.Interfaces;

using Infrastructure.Data;

using Microsoft.EntityFrameworkCore;



namespace Infrastructure.Repositories

{

    public class DisbursementRepository : IDisbursement

    {

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;



        public DisbursementRepository(IDbContextFactory<ApplicationDbContext> contextFactory)

        {

            _contextFactory = contextFactory;

        }



        // FIX: Returns ALL records regardless of balance for the main Dashboard/Grid

        public async Task<List<Disbursement>> GetAllDisbursementsAsync()

        {

            using var context = await _contextFactory.CreateDbContextAsync();

           

            return await context.Disbursements

                .Include(i => i.LoanApplication)

                    .ThenInclude(l => l.Borrower)

                .Include(i => i.PaymentModality)

                .Include(i => i.Payments)

                .Where(d => d.IsActive)

                .OrderByDescending(d => d.CreatedAt)

                .ToListAsync();

        }



        // USE THIS: Only for "New Payment" dropdowns to hide completed loans

        public async Task<List<Disbursement>> GetDisbursementsWithBalanceAsync()

        {

            using var context = await _contextFactory.CreateDbContextAsync();

           

            var data = await context.Disbursements

                .Include(i => i.LoanApplication)

                    .ThenInclude(l => l.Borrower)

                .Include(i => i.PaymentModality)

                .Include(i => i.Payments)

                .Where(d => d.IsActive)

                .ToListAsync();



            return data.Select(d =>

            {

                decimal totalPaid = d.Payments?

                    .Where(p => p.IsActive)

                    .Sum(p => (decimal?)p.Amount ?? 0) ?? 0;



                // We calculate remaining balance in-memory for the selection UI

                d.Amount = d.Amount - totalPaid;

                return d;

            })

            .Where(d => d.Amount > 0.1m) // Filters out finished loans

            .ToList();

        }



        public async Task<Disbursement?> GetDisbursementByIdAsync(int id)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Disbursements

                .Include(i => i.LoanApplication)

                    .ThenInclude(l => l.Borrower)

                .Include(i => i.PaymentModality)

                .Include(i => i.Payments)

                .FirstOrDefaultAsync(i => i.Id == id);

        }



        public async Task CreateDisbursementAsync(CreateDisbursementDTO disbursementDTO)

        {

            using var context = await _contextFactory.CreateDbContextAsync();



            var modality = await context.PaymentModalities

                .FirstOrDefaultAsync(m => m.Id == disbursementDTO.PaymentModalityId);

           

            string mode = modality?.Mode?.ToLower() ?? "monthly";

            DateTime startDate = disbursementDTO.StartDate ?? DateTime.UtcNow;

            int n = disbursementDTO.TotalInstallments > 0 ? disbursementDTO.TotalInstallments : 1;



            // Interest calculation: Interest is applied per installment period

            decimal interestPerPeriod = disbursementDTO.PrincipalOffered * (disbursementDTO.InterestRate / 100);

            decimal totalInterest = interestPerPeriod * n;

            decimal totalAmount = disbursementDTO.PrincipalOffered + totalInterest;



            DateTime calculatedEndDate = mode switch

            {

                "daily" => startDate.AddDays(n),

                "weekly" => startDate.AddDays(n * 7),

                "monthly" => startDate.AddMonths(n),

                "yearly" => startDate.AddYears(n),

                _ => startDate.AddMonths(n)

            };



            var disbursement = new Disbursement

            {

                LoanApplicationId = disbursementDTO.LoanApplicationId,

                PaymentModalityId = disbursementDTO.PaymentModalityId,

                PrincipalOffered = disbursementDTO.PrincipalOffered,

                InterestRate = disbursementDTO.InterestRate,

                TotalInstallments = n,

                Amount = totalAmount,

                StartDate = startDate,

                EndDate = calculatedEndDate,

                IsActive = true,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow

            };



            context.Disbursements.Add(disbursement);

            await context.SaveChangesAsync();

        }

    }

}