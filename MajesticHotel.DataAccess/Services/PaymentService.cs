using MajesticHotel.DataAccess.Repository.IRepository;
using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Repository.IRepository;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace MajesticHotel.Utility.Services
{
    public class PaymentService : IPaymentService
    {        
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }
        public async Task<Booking> CreateOrUpdatePaymentIntent(int bookingId)
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:Secretkey"];

            var booking = await _unitOfWork.Booking.GetAsync(u => u.Id == bookingId, tracked:false);

            if (booking == null) return null;
                

            var service = new PaymentIntentService();

            PaymentIntent intent;

            if (string.IsNullOrEmpty(booking.PaymentIntentId)) // incase of create PaymentIntent 
            {
                var options = new PaymentIntentCreateOptions()
                {
                    Amount = (long)(booking.TotalPrice * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>() { "card" }
                };

                intent = await service.CreateAsync(options);

                booking.PaymentIntentId = intent.Id;
                booking.ClientSecret = intent.ClientSecret;
            }
            else //incase of update
            {
                var options = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)(booking.TotalPrice * 100)
                };

                await service.UpdateAsync(booking.PaymentIntentId, options);

            }

            booking.PaymentStatus = "Approved";
            await _unitOfWork.Booking.UpdateAsync(booking);
            await _unitOfWork.SaveAsync();

            return booking;
        }
    }
}
