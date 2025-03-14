using MajesticHotel.Models;

namespace MajesticHotel.Utility.Services
{
    public interface IPaymentService
    {
        Task<Booking> CreateOrUpdatePaymentIntent(int bookingId);
    }
}
