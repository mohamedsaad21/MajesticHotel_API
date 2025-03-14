using AutoMapper;
using MajesticHotel.DataAccess.Repository.IRepository;
using MajesticHotel.Models;
using MajesticHotel_API.Services.IServices;
using MajesticHotel_HotelAPI.Models.Dto.Rooms;
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
    public class RoomsController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        public RoomsController(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            this._response = new();
            _imageService = imageService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetRooms(int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                var rooms = _mapper.Map<IEnumerable<RoomDTO>>(await _unitOfWork.Room.GetAllAsync(pageSize: pageSize, pageNumber: pageNumber));
                foreach (var room in rooms)
                {
                    room.Images = _imageService.GetImageUrls("room", room.Id);
                }          
                _response.Result = rooms;
                Pagination pagination = new Pagination() { PageNumber = pageNumber, PageSize = pageSize };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

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
        [HttpGet("AvailableRooms")]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetAvailableRooms(int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                var rooms = _mapper.Map<IEnumerable<RoomDTO>>(await _unitOfWork.Room.GetAllAsync(u => u.IsAvailable == true, pageSize:pageSize, pageNumber:pageNumber));
                foreach (var room in rooms)
                {
                    room.Images = _imageService.GetImageUrls("room", room.Id);
                }
                _response.Result = rooms;
                Pagination pagination = new Pagination() { PageNumber = pageNumber, PageSize = pageSize };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

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

        [HttpGet("{id:int}", Name = "GetRoom")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetRoom(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == id);
                if (room == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                
                var roomDTO = _mapper.Map<RoomDTO>(room);
                roomDTO.Images = _imageService.GetImageUrls("Room", id);
                _response.Result = roomDTO;
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
        public async Task<ActionResult<APIResponse>> CreateRoom([FromForm] RoomCreateDTO RoomDTO, List<IFormFile>? files)
        {
            try
            {
                if (RoomDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = _mapper.Map<Room>(RoomDTO);
                room.CreatedAt = DateTime.Now;
                await _unitOfWork.Room.CreateAsync(room);
                await _unitOfWork.SaveAsync();

                await _imageService.UploadImagesAsync(files, "Room", room.Id);
                _response.Result = CreatedAtRoute("GetRoom", new { Id = room.Id }, room);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteRoom")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> DeleteRoom(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == id);
                if (room == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                await _unitOfWork.Room.RemoveAsync(room);
                await _unitOfWork.SaveAsync();

                _imageService.DeleteImages("Room", id);
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
        [HttpPut("{id:int}", Name = "UpdateRoom")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdateRoom(int id, [FromBody] RoomUpdateDTO RoomDTO)
        {
            try
            {
                if (id == 0 || RoomDTO == null || id != RoomDTO.Id)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == id, tracked: false);
                if (room == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                room = _mapper.Map<Room>(RoomDTO);
                await _unitOfWork.Room.UpdateAsync(room);
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

        [HttpPatch("{id:int}", Name = "UpdatePartialRoom")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdatePartialRoom(int id, JsonPatchDocument<RoomUpdateDTO> patchDTO)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == id, tracked: false);
                if (room == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var RoomDTO = _mapper.Map<RoomUpdateDTO>(room);
                patchDTO.ApplyTo(RoomDTO);
                room = _mapper.Map<Room>(RoomDTO);

                await _unitOfWork.Room.UpdateAsync(room);
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
    }
}
