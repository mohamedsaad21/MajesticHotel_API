using System.ComponentModel.DataAnnotations.Schema;

namespace MajesticHotel_HotelAPI.Models.Dto.Bookings
{
    public class BookingCreateDTO
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int Adults { get; set; }
        public int Children { get; set; }
    }
}
