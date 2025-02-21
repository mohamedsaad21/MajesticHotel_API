using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Models.Dto.Hotels;
using System.Linq.Expressions;

namespace MajesticHotel_HotelAPI.Repository.IRepository
{
    public interface IHotelRepository : IRepository<Hotel>
    {
        Task<IEnumerable<HotelsDTO>> GetAllWithAmenitiesAsync(Expression<Func<HotelsDTO, bool>>? filter = null, int pageSize = 0, int pageNumber = 1);
        Task<HotelsDTO> GetWithAmenitiesAsync(Expression<Func<HotelsDTO, bool>> filter = null, bool tracked = true);
    }
}
