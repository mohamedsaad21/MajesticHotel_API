using MajesticHotel_HotelAPI.Repository.IRepository;

namespace MajesticHotel.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IAmenityRepository Amenity { get; }
        IBookingRepository Booking { get; }
        IBookingHeaderRepository BookingHeader { get; }
        ICityRepository City { get; }
        IHotelRepository Hotel { get; }
        IRoomClassRepository RoomClass { get; }
        IRoomRepository Room { get; }
        Task SaveAsync();
    }
}
