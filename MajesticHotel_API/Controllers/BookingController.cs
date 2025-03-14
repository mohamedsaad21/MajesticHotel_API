using AutoMapper;
using MajesticHotel.DataAccess.Repository.IRepository;
using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Models.Dto.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Net;
using System.Text.Json;

namespace MajesticHotel_HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingController(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            this._response = new();
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
                    await _unitOfWork.Booking.GetAllAsync(u => u.UserId == userId, pageSize:pageSize, pageNumber:pageNumber);

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
                _response.Result = _mapper.Map<BookingDTO>(await _unitOfWork.Booking.GetAsync(u => u.Id == id && u.UserId == userId));
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
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == bookingDTO.RoomId);
                if(room == null || !room.IsAvailable)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Room Is Not Available!" };
                    return BadRequest(_response);
                }
                var booking = _mapper.Map<Booking>(bookingDTO);
                booking.UserId = User.FindFirst("uid")?.Value!;
                var roomClass = await _unitOfWork.RoomClass.GetAsync(u => u.Id == room.RoomClassId);
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


                var options = new SessionCreateOptions
                {
                    SuccessUrl = $"https://localhost:44361/api/room/{room.Id}",
                    CancelUrl = $"https://localhost:44361/api/room/{room.Id}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(booking.TotalPrice * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = roomClass.Name
                        }                       
                    },
                    Quantity = 1
                };
                options.LineItems.Add(sessionLineItem);
                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.BookingHeader.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
                booking.PaymentStatus = "paid";
                room.IsAvailable = false;
                await _unitOfWork.Booking.CreateAsync(booking);
                await _unitOfWork.SaveAsync(); 

                Response.Headers.Add("Location", session.Url);


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
                var booking = await _unitOfWork.Booking.GetAsync(u => u.Id == id && u.UserId == userId);
                if(booking == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == booking.RoomId);
                room.IsAvailable = true;
                await _unitOfWork.Booking.RemoveAsync(booking);
                await _unitOfWork.SaveAsync();

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
                var booking = await _unitOfWork.Booking.GetAsync(u => u.Id == id, tracked: false);
                var oldRoom = await _unitOfWork.Room.GetAsync(u => u.Id == booking.RoomId);
                if(booking == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                
                booking = _mapper.Map<Booking>(bookingDTO);
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == bookingDTO.RoomId);

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

                var roomClass = await _unitOfWork.RoomClass.GetAsync(u => u.Id == room.RoomClassId);
                if (booking.Adults > roomClass.AdultsCapacity || booking.Children > roomClass.ChildrenCapacity)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Guest capacity exceeded for this room" };
                    return BadRequest(_response);
                }

                var duration = (booking.CheckOutDate.Date - booking.CheckInDate.Date).Days;
                booking.TotalPrice = duration * roomClass.PricePerNight;

                if (oldRoom.Id != room.Id)
                {
                    oldRoom.IsAvailable = true;
                    room.IsAvailable = false;
                }

                await _unitOfWork.Booking.UpdateAsync(booking);
                await _unitOfWork.SaveAsync();


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
                var booking = await _unitOfWork.Booking.GetAsync(u => u.Id == id, tracked: false);
                var oldRoom = await _unitOfWork.Room.GetAsync(u => u.Id == booking.RoomId);
                var bookingDTO = _mapper.Map<BookingUpdateDTO>(booking);
                patchDTO.ApplyTo(bookingDTO);
                booking = _mapper.Map<Booking>(bookingDTO);
                booking.UserId = User.FindFirst("uid")?.Value!;
                if(booking == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var room = await _unitOfWork.Room.GetAsync(u => u.Id == booking.RoomId);
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
                var roomClass = await _unitOfWork.RoomClass.GetAsync(u => u.Id == room.RoomClassId);
                if (booking.Adults > roomClass.AdultsCapacity || booking.Children > roomClass.ChildrenCapacity)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Guest capacity exceeded for this room" };
                    return BadRequest(_response);
                }
                var duration = (booking.CheckOutDate - booking.CheckInDate).Days;
                booking.TotalPrice = duration * roomClass.PricePerNight;


                if (oldRoom.Id != room.Id)
                {
                    oldRoom.IsAvailable = true;
                    room.IsAvailable = false;
                }
                await _unitOfWork.Booking.UpdateAsync(booking);
                await _unitOfWork.SaveAsync();

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
