using System.ComponentModel.DataAnnotations;

namespace MajesticHotel.Models
{
    public class Amenity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
