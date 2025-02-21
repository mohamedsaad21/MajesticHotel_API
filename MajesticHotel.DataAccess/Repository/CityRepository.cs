using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Data;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Repository.IRepository;

namespace MajesticHotel_HotelAPI.Repository
{
    public class CityRepository : Repository<City>, ICityRepository
    {
        private readonly ApplicationDbContext _db;
        public CityRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
