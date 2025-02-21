using AutoMapper;
using MajesticHotel.Models;
using MajesticHotel_HotelAPI.Helpers;
using MajesticHotel_HotelAPI.Models;
using MajesticHotel_HotelAPI.Services.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MajesticHotel_HotelAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private JWT _jwt;
        public AuthService(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IOptions<JWT>jwt)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _jwt = jwt.Value;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByNameAsync(model.Username) != null)
                return new AuthModel { Message = "Username already registered!" };
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return new AuthModel { Message = "Email already registered!" };

            var user = _mapper.Map<ApplicationUser>(model);
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach(var error in result.Errors)
                {
                    errors += $"{error.Description},";
                }
                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, Roles.Guest);

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel 
            { 
                IsAuthenticated = true,
                Email = model.Email,
                Username = model.Username,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Roles = new List<string> { Roles.Guest },
                ExpiresOn = jwtSecurityToken.ValidTo    
            };
        }
        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }
            var jwtSecurityToken = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Email = user.Email!;
            authModel.Username = user.UserName!;
            authModel.Roles = roles.ToList();
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;

            return authModel;
            
        } 
        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach(var role in roles)
            {
                roleClaims.Add(new Claim("roles", role));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("uid", user.Id)
            }.Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer:_jwt.Issuer,
                audience:_jwt.Audience,
                claims:claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials:signingCredentials                
                );
            return jwtSecurityToken;
        }
        public async Task<string>AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid User ID or Role";
            
            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";
         
            var result = await _userManager.AddToRoleAsync(user, model.Role);
            return result.Succeeded ? string.Empty : "Something went wrong";
        }
    }
}
