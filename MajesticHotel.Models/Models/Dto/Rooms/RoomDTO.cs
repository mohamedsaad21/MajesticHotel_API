using System.ComponentModel.DataAnnotations.Schema;

namespace MajesticHotel_HotelAPI.Models.Dto.Rooms
{
    public class RoomDTO
    {
        public int Id { get; set; }

        public int RoomClassId { get; set; }

        public int HotelId { get; set; }

        public bool IsAvailable { get; set; }

        public List<string> Images { get; set; }
    }
}
