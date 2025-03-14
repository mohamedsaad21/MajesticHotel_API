using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Data;
using MajesticHotel_HotelAPI.Repository.IRepository;

namespace MajesticHotel_HotelAPI.Repository
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
