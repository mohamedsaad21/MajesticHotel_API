using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MajesticHotel_HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);
            if(!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(new {token = result.Token, expires = result.ExpiresOn });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TokenRequestModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.GetTokenAsync(model);
            if(!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("addrole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRoleToUser([FromBody] AddRoleModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AddRoleAsync(model);
            if(!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }

    }
}
