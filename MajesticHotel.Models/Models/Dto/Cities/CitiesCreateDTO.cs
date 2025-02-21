using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MajesticHotel_HotelAPI.Models.Dto.Cities
{
    public class CitiesCreateDTO
    {
        public string Name { get; set; }
        public string PostalCode { get; set; }
    }
}
