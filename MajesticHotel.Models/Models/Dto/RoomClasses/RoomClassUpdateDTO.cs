namespace MajesticHotel_HotelAPI.Models.Dto.RoomClasses
{
    public class RoomClassUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AdultsCapacity { get; set; }
        public int ChildrenCapacity { get; set; }

        public decimal PricePerNight { get; set; }
    }
}
