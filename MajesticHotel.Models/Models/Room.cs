using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajesticHotel.Models
{
    public class Room
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(RoomClass))]
        public int RoomClassId { get; set; }
        public RoomClass RoomClass { get; set; }

        [ForeignKey(nameof(Hotel))]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public Booking Booking { get; set; }
    }
}
