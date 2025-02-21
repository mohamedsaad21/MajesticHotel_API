using System.ComponentModel.DataAnnotations;

namespace MajesticHotel_HotelAPI.Models.Dto.RoomClasses
{
    public class RoomClassCreateDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int AdultsCapacity { get; set; }
        [Required]
        public int ChildrenCapacity { get; set; }
        [Required]
        public decimal PricePerNight { get; set; }

    }
}
