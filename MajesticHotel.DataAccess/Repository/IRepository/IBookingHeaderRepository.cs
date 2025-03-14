using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajesticHotel.DataAccess.Repository.IRepository
{
    public interface IBookingHeaderRepository
    {
        void UpdateStripePaymentId(int id, string sessionId,string paymentIntentId);
    }
}
