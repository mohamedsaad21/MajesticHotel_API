﻿using AutoMapper;
using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Models.Dto.Bookings;
using MajesticHotel_HotelAPI.Repository.IRepository;
using MajesticHotel_HotelAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace MajesticHotel_HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IRoomClassRepository _roomClassRepository;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;

        public BookingController(IBookingRepository db,
            IMapper mapper,
            IRoomRepository roomRepository,
            IPaymentService paymentService,
            IRoomClassRepository roomClassRepository)
        {
            _bookingRepository = db;
            _mapper = mapper;
            _roomRepository = roomRepository;
            this._response = new();
            _roomClassRepository = roomClassRepository;
            _paymentService = paymentService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetBookings(int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                IEnumerable<Booking> bookings = 
                    await _bookingRepository.GetAllAsync(u => u.UserId == userId, pageSize:pageSize, pageNumber:pageNumber);

                Pagination pagination = new Pagination() { PageNumber = pageNumber, PageSize = pageSize };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                _response.Result = _mapper.Map<IEnumerable<BookingDTO>>(bookings);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
        [HttpGet("{id:int}", Name = "GetBooking")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>>GetBooking(int id)
        {
            try
            {
                if(id == 0)
                {
                    _response.StatusCode=HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var userId = User.FindFirst("uid")?.Value;
                _response.Result = _mapper.Map<BookingDTO>(await _bookingRepository.GetAsync(u => u.Id == id && u.UserId == userId));
                if(_response.Result == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> CreateBooking([FromBody] BookingCreateDTO bookingDTO)
        {
            try
            {
                var room = await _roomRepository.GetAsync(u => u.Id == bookingDTO.RoomId);
                if(room == null || !room.IsAvailable)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Room Is Not Available!" };
                    return BadRequest(_response);
                }
                var booking = _mapper.Map<Booking>(bookingDTO);
                booking.UserId = User.FindFirst("uid")?.Value!;
                var roomClass = await _roomClassRepository.GetAsync(u => u.Id == room.RoomClassId);
                if(booking.Adults > roomClass.AdultsCapacity || booking.Children > roomClass.ChildrenCapacity)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Guest capacity exceeded for this room" };
                    return BadRequest(_response);
                }
                if(booking.CheckInDate.Date < DateTime.UtcNow.Date || booking.CheckInDate.Date >= booking.CheckOutDate.Date)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "CheckInDate is Invalid!" };
                    return BadRequest(_response);
                }
                var duration = (bookingDTO.CheckOutDate.Date - booking.CheckInDate).Days;

                booking.TotalPrice = duration * roomClass.PricePerNight;
                booking.PaymentStatus = false;
                room.IsAvailable = false;

                await _bookingRepository.CreateAsync(booking);
                _response.Result = CreatedAtAction("GetBooking", new { Id = booking.Id }, booking);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteBooking")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>>DeleteBooking(int id)
        {
            try
            {
                if(id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var userId = User.FindFirst("uid")?.Value;
                var booking = await _bookingRepository.GetAsync(u => u.Id == id && u.UserId == userId);
                if(booking == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _roomRepository.GetAsync(u => u.Id == booking.RoomId);
                room.IsAvailable = true;
                await _bookingRepository.RemoveAsync(booking);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateBooking")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>>UpdateBooking(int id, [FromBody] BookingUpdateDTO bookingDTO)
        {
            try
            {
                if(id == 0 || bookingDTO == null || bookingDTO.Id != id)
                {
                    _response.StatusCode=HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var booking = await _bookingRepository.GetAsync(u => u.Id == id, tracked: false);
                var oldRoom = await _roomRepository.GetAsync(u => u.Id == booking.RoomId);
                if(booking == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                
                booking = _mapper.Map<Booking>(bookingDTO);
                var room = await _roomRepository.GetAsync(u => u.Id == bookingDTO.RoomId);

                booking.UserId = User.FindFirst("uid")?.Value!;

                if (oldRoom.Id != room.Id && !room.IsAvailable)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Room Is Not Available!" };
                    return BadRequest(_response);
                }

                if (booking.CheckInDate.Date < DateTime.UtcNow.Date || booking.CheckInDate.Date >= booking.CheckOutDate.Date)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "CheckInDate is Invalid!" };
                    return BadRequest(_response);
                }

                var roomClass = await _roomClassRepository.GetAsync(u => u.Id == room.RoomClassId);
                if (booking.Adults > roomClass.AdultsCapacity || booking.Children > roomClass.ChildrenCapacity)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Guest capacity exceeded for this room" };
                    return BadRequest(_response);
                }

                var duration = (booking.CheckOutDate.Date - booking.CheckInDate.Date).Days;
                booking.TotalPrice = duration * roomClass.PricePerNight;

                //var paymentIntent = _paymentService.CreateOrUpdatePaymentIntent(booking.TotalPrice);

                booking.PaymentStatus = false;

                if (oldRoom.Id != room.Id)
                {
                    oldRoom.IsAvailable = true;
                    room.IsAvailable = false;
                }

                await _bookingRepository.UpdateAsync(booking);

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
        [HttpPatch("{id:int}", Name = "UpdatePartialBooking")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdatePartialBooking(int id, JsonPatchDocument<BookingUpdateDTO> patchDTO)
        {
            try
            {
                if(id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var booking = await _bookingRepository.GetAsync(u => u.Id == id, tracked: false);
                var oldRoom = await _roomRepository.GetAsync(u => u.Id == booking.RoomId);
                var bookingDTO = _mapper.Map<BookingUpdateDTO>(booking);
                patchDTO.ApplyTo(bookingDTO);
                booking = _mapper.Map<Booking>(bookingDTO);
                booking.UserId = User.FindFirst("uid")?.Value!;
                if(booking == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _roomRepository.GetAsync(u => u.Id == booking.RoomId);
                if (oldRoom.Id != room.Id && !room.IsAvailable)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Room Is Not Available!" };
                    return BadRequest(_response);
                }
                if (booking.CheckInDate.Date < DateTime.UtcNow.Date || booking.CheckInDate.Date >= booking.CheckOutDate.Date)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "CheckInDate is Invalid!" };
                    return BadRequest(_response);
                }
                var roomClass = await _roomClassRepository.GetAsync(u => u.Id == room.RoomClassId);
                if (booking.Adults > roomClass.AdultsCapacity || booking.Children > roomClass.ChildrenCapacity)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Guest capacity exceeded for this room" };
                    return BadRequest(_response);
                }
                var duration = (booking.CheckOutDate - booking.CheckInDate).Days;
                booking.TotalPrice = duration * roomClass.PricePerNight;

                var paymentIntent = _paymentService.CreateOrUpdatePaymentIntent(booking.TotalPrice);

                booking.PaymentStatus = false;
                if (oldRoom.Id != room.Id)
                {
                    oldRoom.IsAvailable = true;
                    room.IsAvailable = false;
                }
                await _bookingRepository.UpdateAsync(booking);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
    }
}
