using AutoMapper;
using MajesticHotel.DataAccess.Repository.IRepository;
using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Models.Dto.Amenities;
using MajesticHotel_HotelAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace MajesticHotel_HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public AmenitiesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            this._response = new();
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetAmenities(int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                IEnumerable<AmenityDTO> AmenityList = _mapper.Map<List<AmenityDTO>>
                    (await _unitOfWork.Amenity.GetAllAsync(pageSize:pageSize, pageNumber:pageNumber));

                Pagination pagination = new Pagination() { PageNumber = pageNumber, PageSize = pageSize };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                _response.Result = AmenityList;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            } catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
        [HttpGet("{id:int}", Name = "GetAmenity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetAmenity(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var Amenity = await _unitOfWork.Amenity.GetAsync(u => u.Id == id, tracked: false);
                if (Amenity == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<AmenityDTO>(Amenity);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            } catch (Exception ex)
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
        public async Task<ActionResult<APIResponse>> CreateAmenity([FromBody] AmenityCreateDTO AmenityDTO)
        {
            try
            {
                if (AmenityDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                if(await _unitOfWork.Amenity.GetAsync(u => u.Name == AmenityDTO.Name, tracked:false) != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Amenity already Exists!" };
                    return BadRequest(_response);
                }
                var Amenity = _mapper.Map<Amenity>(AmenityDTO);
                await _unitOfWork.Amenity.CreateAsync(Amenity);
                await _unitOfWork.SaveAsync();
                _response.Result = CreatedAtAction("GetAmenity", new { id = Amenity.Id }, Amenity);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            } catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
        [HttpDelete("{id:int}", Name = "DeleteAmenity")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> DeleteAmenity(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var Amenity = await _unitOfWork.Amenity.GetAsync(u => u.Id == id, tracked: false);
                if (Amenity == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                await _unitOfWork.Amenity.RemoveAsync(Amenity);
                await _unitOfWork.SaveAsync();

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            } catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateAmenity")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdateAmenity(int id, [FromBody] AmenityUpdateDTO AmenityDTO)
        {
            try
            {
                if (id == 0 || AmenityDTO == null || id != AmenityDTO.Id)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                if (await _unitOfWork.Amenity.GetAsync(u => u.Name == AmenityDTO.Name, tracked: false) != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Amenity already Exists!" };
                    return BadRequest(_response);
                }
                var Amenity = await _unitOfWork.Amenity.GetAsync(u => u.Id == id, tracked: false);
                if (Amenity == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                Amenity = _mapper.Map<Amenity>(AmenityDTO);
                await _unitOfWork.Amenity.UpdateAsync(Amenity);
                await _unitOfWork.SaveAsync();

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialAmenity")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdatePartialAmenity(int id, JsonPatchDocument<AmenityUpdateDTO> patchDTO)
        {
            try
            {
                if(id == 0 || patchDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var Amenity = await _unitOfWork.Amenity.GetAsync(u => u.Id ==id, tracked: false);
                if (Amenity == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var AmenityDTO = _mapper.Map<AmenityUpdateDTO>(Amenity);
                patchDTO.ApplyTo(AmenityDTO);
                if (await _unitOfWork.Amenity.GetAsync(u => u.Name == AmenityDTO.Name, tracked: false) != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Amenity already Exists!" };
                    return BadRequest(_response);
                }
                Amenity = _mapper.Map<Amenity>(AmenityDTO);
                await _unitOfWork.Amenity.UpdateAsync(Amenity);
                await _unitOfWork.SaveAsync();
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

    }
}
