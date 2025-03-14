using MajesticHotel.DataAccess.Repository.IRepository;
using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Data;
using MajesticHotel_HotelAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajesticHotel.DataAccess.Repository
{
    public class BookingHeaderRepository : Repository<Booking>, IBookingHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public BookingHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void UpdateStripePaymentId(int id, string sessionId,string paymentIntentId)
        {
            var bookingFromDb = _db.Bookings.FirstOrDefault(u => u.Id == id);
            if(!string.IsNullOrEmpty(sessionId) )
            {
                bookingFromDb.SessionId = sessionId;
            }
            if(!string.IsNullOrEmpty(paymentIntentId) )
            {
                bookingFromDb.PaymentIntentId = paymentIntentId;
            }
        }
    }
}
