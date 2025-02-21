using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Models.Dto.Bookings;
using MajesticHotel_HotelAPI.Repository.IRepository;
using MajesticHotel_HotelAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Net;

namespace MajesticHotel_HotelAPI.Controllers
{    
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        protected APIResponse _response;
        private readonly IConfiguration _configuration;
        public PaymentsController(IBookingRepository bookingRepository, IConfiguration configuration)
        {
            _bookingRepository = bookingRepository;
            this._response = new();
            _configuration = configuration;
        }
        [HttpPost("pay/{bookingId}")]
        public async Task<IActionResult> ProcessPayment(int bookingId)
        {
            var booking = await _bookingRepository.GetAsync(u => u.Id == bookingId);
            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.PaymentStatus == true)
                return BadRequest("Booking is already paid.");

            if (booking.TotalPrice < (decimal)0.50)
                return BadRequest("Total Price must be at least $0.50.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(booking.TotalPrice * 100), // Convert to cents
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" }
            };
            StripeConfiguration.ApiKey = _configuration["StripeSettings:Secretkey"];
            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            booking.PaymentIntentId = paymentIntent.Id;
            booking.PaymentStatus = true;

            await _bookingRepository.UpdateAsync(booking);

            return Ok(new { PaymentIntentId = paymentIntent.Id, ClientSecret = paymentIntent.ClientSecret });
        }
    }
}
