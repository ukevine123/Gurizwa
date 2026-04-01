using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class PaymentModalityRepository: IPaymentModality
    {
        private readonly ApplicationDbContext dbContext;
        public PaymentModalityRepository(ApplicationDbContext context)
        {
           dbContext=context; 
        }
        // public  async Task<List<PaymentModality>> GetAllPaymentModalitysAsync()
        // {
        //   List<PaymentModality> _paymentModality = dbContext.PaymentModalities.ToList();
        //   return _paymentModality;
        // }

        public async Task<List<PaymentModality>> GetAllPaymentModalitysAsync()
{
    // Use ToListAsync() and await it
    return await dbContext.PaymentModalities.ToListAsync();
}
        public async Task <PaymentModality> GetPaymentModalityById(int Id)
        {
            return  dbContext.PaymentModalities.FirstOrDefault(t => t.Id == Id);
        }
         public async Task CreatePaymentModality(CreatePaymentModalityDTO paymentModalityDTO)
        {
            var _paymentModality = new PaymentModality
            {
                Mode = paymentModalityDTO.Mode,
                // CreatedBy = "Admin" 
            };
            dbContext.PaymentModalities.Add(_paymentModality);
            dbContext.SaveChanges();
        }
       
    }
    }   
    
    
        