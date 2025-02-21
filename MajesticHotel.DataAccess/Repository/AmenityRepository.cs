using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Data;
using MajesticHotel_HotelAPI.Repository.IRepository;

namespace MajesticHotel_HotelAPI.Repository
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        public AmenityRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
