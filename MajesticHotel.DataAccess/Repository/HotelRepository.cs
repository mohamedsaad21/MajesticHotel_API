using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Data;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Models.Dto.Amenities;
using MajesticHotel_HotelAPI.Models.Dto.Hotels;
using MajesticHotel_HotelAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace MajesticHotel_HotelAPI.Repository
{
    public class HotelRepository : Repository<Hotel>, IHotelRepository
    {
        private readonly ApplicationDbContext _db;
        public HotelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<HotelsDTO>> GetAllWithAmenitiesAsync(Expression<Func<HotelsDTO, bool>>? filter = null, int pageSize = 0, int pageNumber = 1)
        {
            IQueryable<HotelsDTO> query = _db.Hotels.Include(h => h.City).Include(h => h.HotelAmenities).ThenInclude(ha => ha.Amenity).AsNoTracking()
               .Select(h => new HotelsDTO
               {
                   Id = h.Id,
                   Name = h.Name,
                   Description = h.Description,
                   Phone = h.Phone,
                   Email = h.Email,
                   CityId = h.CityId,
                   Amenities = h.HotelAmenities
                   .Select(ha => new Amenity { Id = ha.Amenity.Id, Name = ha.Amenity.Name, Description = ha.Amenity.Description }).ToList()
               });            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (pageSize > 0)
            {
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
            }
            query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            return query;
        }
        public async Task<HotelsDTO> GetWithAmenitiesAsync(Expression<Func<HotelsDTO, bool>> filter = null, bool tracked = true)
        {
            IQueryable<HotelsDTO> query = _db.Hotels.Include(h => h.City).Include(h => h.HotelAmenities).ThenInclude(ha => ha.Amenity)
               .Select(h => new HotelsDTO
               {
                   Id = h.Id,
                   Name = h.Name,
                   Description = h.Description,
                   Phone = h.Phone,
                   Email = h.Email,
                   CityId = h.CityId,
                   Amenities = h.HotelAmenities
                   .Select(ha => new Amenity { Id = ha.Amenity.Id, Name = ha.Amenity.Name, Description = ha.Amenity.Description }).ToList()
               });
            if(filter != null)
            {
                query = query.Where(filter);
            }
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }
    }
}
