using AutoMapper;
using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Models.Dto.Amenities;
using MajesticHotel_HotelAPI.Models.Dto.Bookings;
using MajesticHotel_HotelAPI.Models.Dto.Cities;
using MajesticHotel_HotelAPI.Models.Dto.Hotels;
using MajesticHotel_HotelAPI.Models.Dto.RoomClasses;
using MajesticHotel_HotelAPI.Models.Dto.Rooms;

namespace MajesticHotel_HotelAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<City, CitiesDTO>().ReverseMap();
            CreateMap<City, CitiesCreateDTO>().ReverseMap();
            CreateMap<City, CitiesUpdateDTO>().ReverseMap();

            CreateMap<Hotel, HotelsDTO>().ReverseMap().ForMember(src => src.ImageUrl, opt => opt.Ignore());
            CreateMap<Hotel, HotelsCreateDTO>().ReverseMap().ForMember(src => src.ImageUrl, opt => opt.Ignore());
            CreateMap<Hotel, HotelsUpdateDTO>().ReverseMap().ForMember(src => src.ImageUrl, opt => opt.Ignore());

            CreateMap<RoomClass, RoomClassDTO>().ReverseMap();
            CreateMap<RoomClass, RoomClassCreateDTO>().ReverseMap();
            CreateMap<RoomClass, RoomClassUpdateDTO>().ReverseMap();

            CreateMap<Room, RoomDTO>().ReverseMap();
            CreateMap<Room, RoomCreateDTO>().ReverseMap();
            CreateMap<Room, RoomUpdateDTO>().ReverseMap();

            CreateMap<Booking, BookingDTO>().ReverseMap();
            CreateMap<Booking, BookingCreateDTO>().ReverseMap();
            CreateMap<Booking, BookingUpdateDTO>().ReverseMap();

            CreateMap<Amenity, AmenityDTO>().ReverseMap();
            CreateMap<Amenity, AmenityCreateDTO>().ReverseMap();
            CreateMap<Amenity, AmenityUpdateDTO>().ReverseMap();

            CreateMap<RegisterModel, ApplicationUser>();
        }
    }
}
