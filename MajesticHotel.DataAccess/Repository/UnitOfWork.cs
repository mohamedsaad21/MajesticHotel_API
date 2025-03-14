using MajesticHotel.DataAccess.Repository.IRepository;
using MajesticHotel_API.Services.IServices;
using MajesticHotel_HotelAPI.Data;
using MajesticHotel_HotelAPI.Repository;
using MajesticHotel_HotelAPI.Repository.IRepository;

namespace MajesticHotel.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;
        public IAmenityRepository Amenity { get; private set; }

        public IBookingRepository Booking { get; private set; }

        public IBookingHeaderRepository BookingHeader { get; private set; }

        public ICityRepository City { get; private set; }

        public IHotelRepository Hotel { get; private set; }

        public IRoomClassRepository RoomClass { get; private set; }

        public IRoomRepository Room { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Amenity = new AmenityRepository(db);
            Booking = new BookingRepository(db);
            City = new CityRepository(db);
            Hotel = new HotelRepository(db, _imageService);
            RoomClass = new RoomClassRepository(db);
            Room = new RoomRepository(db);
            BookingHeader = new BookingHeaderRepository(db);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
