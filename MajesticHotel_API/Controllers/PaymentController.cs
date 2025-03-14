using AutoMapper;
using MajesticHotel.Models;
using MajesticHotel.Utility.Services;
using MajesticHotel_HotelAPI.Models.Dto.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MajesticHotel_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        protected APIResponse _response;

        public PaymentController(IPaymentService paymentService, IMapper mapper)
        {
            _paymentService = paymentService;
            this._response = new();
            _mapper = mapper;
        }
        [HttpPost("{bookingId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> CreateOrUpdatePaymentIntent(int bookingId)
        {
            try
            {
                var booking = await _paymentService.CreateOrUpdatePaymentIntent(bookingId);
                if (booking == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Your Problem With Your Booking" };
                    return BadRequest(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<BookingDTO>(booking);
            }
            catch (Exception ex) {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message.ToString() };
            }
            return _response;
        }
    }
}
