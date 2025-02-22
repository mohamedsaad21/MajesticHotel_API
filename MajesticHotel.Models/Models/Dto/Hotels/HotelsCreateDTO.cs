using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace MajesticHotel_HotelAPI.Models.Dto.Hotels
{
    public class HotelsCreateDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }

        [Required]
        public int CityId { get; set; }
        public List<int>? HotelAmenitiesIds { get; set; }
    }
}
