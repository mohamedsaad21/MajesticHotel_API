using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Services.IServices;
using Stripe;

namespace MajesticHotel_HotelAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly string _stripeSecretKey;

        public PaymentService(IConfiguration configuration)
        {
            _stripeSecretKey = configuration["Stripe:SecretKey"]!;
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        public async Task<PaymentIntentResponse> CreateOrUpdatePaymentIntent(decimal amount)
        {
            var paymentIntentService = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" }
            };

            var paymentIntent = await paymentIntentService.CreateAsync(options);
            return new PaymentIntentResponse
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret
            };
        }

        public async Task<string> GetPaymentStatus(string paymentIntentId)
        {
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);
            return paymentIntent.Status; // e.g., "succeeded", "requires_payment_method"
        }

        public async Task<bool> RefundPayment(string paymentIntentId)
        {
            try
            {
                var refundService = new RefundService();
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId
                };

                var refund = await refundService.CreateAsync(options);
                return refund.Status == "succeeded";
            }
            catch
            {
                return false;
            }
        }
    }

}