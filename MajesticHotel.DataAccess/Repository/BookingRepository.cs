using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Data;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Repository.IRepository;

namespace MajesticHotel_HotelAPI.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
