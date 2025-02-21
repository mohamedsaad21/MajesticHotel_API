using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajesticHotel_HotelAPI.Models.Dto.Hotels
{
    public class HotelsUpdateDTO
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string? ImageUrl { get; set; }
        [Required]
        public int CityId { get; set; }
        public List<int>? HotelAmenitiesIds { get; set; } = new List<int>();

    }
}
