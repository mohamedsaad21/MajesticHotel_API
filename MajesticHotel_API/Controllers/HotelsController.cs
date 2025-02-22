using AutoMapper;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MajesticHotel_HotelAPI.Models.Dto.Hotels;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using MajesticHotel.Models;
using MajesticHotel_API.Services.IServices;

namespace MajesticHotel_HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IHotelRepository _db;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        public HotelsController(IHotelRepository db, IMapper mapper, IImageService imageService)
        {
            _db = db;
            _mapper = mapper;
            this._response = new();
            _imageService = imageService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetHotels(int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                _response.Result = await _db.GetAllWithAmenitiesAsync(pageSize:pageSize, pageNumber:pageNumber);

                Pagination pagination = new Pagination() { PageNumber = pageNumber, PageSize = pageSize };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }


        [HttpGet("{id:int}", Name = "GetHotel")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetHotel(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var Hotel = await _db.GetWithAmenitiesAsync(u => u.Id == id, tracked: false);
                if (Hotel == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = Hotel;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> CreateHotel([FromForm]HotelsCreateDTO HotelDTO, List<IFormFile>? files)
        {
            try
            {
                if (HotelDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var hotel = _mapper.Map<Hotel>(HotelDTO);
                if (HotelDTO.HotelAmenitiesIds != null)
                {
                    hotel.HotelAmenities = HotelDTO.HotelAmenitiesIds.Select(x => new HotelAmenities { AmenityId = x }).ToList();
                }
                await _db.CreateAsync(hotel);

                await _imageService.UploadImagesAsync(files, "Hotel", hotel.Id);

                _response.Result = CreatedAtRoute("GetHotel", new { Id = hotel.Id }, hotel);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteHotel")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> DeleteHotel(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var hotel = await _db.GetAsync(u => u.Id == id);
                if (hotel == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                await _db.RemoveAsync(hotel);

                _imageService.DeleteImages("Hotel", id);
                
                _response.StatusCode=HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
        [HttpPut("{id:int}", Name = "UpdateHotel")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdateHotel(int id, [FromBody] HotelsUpdateDTO HotelDTO)
        {
            try
            {
                if (id == 0 || HotelDTO == null || id != HotelDTO.Id)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var hotelDTOWithAmenities = await _db.GetWithAmenitiesAsync(u => u.Id == id);
                if (hotelDTOWithAmenities == null)
                {                    
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var hotel = _mapper.Map<Hotel>(HotelDTO);
                if (HotelDTO.HotelAmenitiesIds != null)
                {
                    var ExistingAmenities = hotelDTOWithAmenities.Amenities.Select(A => new HotelAmenities { HotelId = hotelDTOWithAmenities.Id, AmenityId = A.Id }).ToList();
                    var NewAmenities = HotelDTO.HotelAmenitiesIds.Select(x => new HotelAmenities { HotelId = hotelDTOWithAmenities.Id, AmenityId = x }).ToList();
                    var ChosenAmenities = NewAmenities.Except(ExistingAmenities).ToList();
                    hotel.HotelAmenities = ChosenAmenities;
                }
                await _db.UpdateAsync(hotel);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialHotel")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdatePartialHotel(int id, JsonPatchDocument<HotelsUpdateDTO> patchDTO)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var hotel = await _db.GetAsync(u => u.Id == id, tracked: false);
                if (hotel == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var hotelDTO = _mapper.Map<HotelsUpdateDTO>(hotel);
                patchDTO.ApplyTo(hotelDTO);
                hotel = _mapper.Map<Hotel>(hotelDTO);

                await _db.UpdateAsync(hotel);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
    }
}
