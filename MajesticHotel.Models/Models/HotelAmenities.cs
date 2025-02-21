using System.ComponentModel.DataAnnotations.Schema;

namespace MajesticHotel.Models
{
    public class HotelAmenities
    {
        [ForeignKey(nameof(Hotel))]
        public int HotelId { get; set; }
        [ForeignKey(nameof(Amenity))]
        public int AmenityId { get; set; }

        public Hotel Hotel { get; set; }
        public Amenity Amenity { get; set; }
    }
}
