using MajesticHotel.Models;

namespace MajesticHotel_HotelAPI.Services.IServices
{
    public interface IPaymentService
    {
        Task<PaymentIntentResponse> CreateOrUpdatePaymentIntent(decimal amount);
        Task<string> GetPaymentStatus(string paymentIntentId);
        Task<bool> RefundPayment(string paymentIntentId);
    }
}
