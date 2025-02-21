using System.ComponentModel.DataAnnotations.Schema;

namespace MajesticHotel_HotelAPI.Models.Dto.Bookings
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public DateTime CreatedAt { get; set; }


        public decimal TotalPrice { get; set; }

        public bool PaymentStatus { get; set; }
        public string PaymentIntentId { get; set; }

        public int Adults { get; set; }
        public int Children { get; set; }

    }
}
