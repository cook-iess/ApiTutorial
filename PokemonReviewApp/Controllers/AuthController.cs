using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PokemonReviewApp.Configurations;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        //private readonly JwtConfig _jwtConfig;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            //_jwtConfig = jwtConfig;

        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegReqDto reqDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(reqDto.Email);

            if (existingUser != null)
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Email already in use"
                    }
                });

            var newUser = new IdentityUser()
            {
                Email = reqDto.Email,
                UserName = reqDto.UserName
            };

            var isCreated = await _userManager.CreateAsync(newUser, reqDto.Password);

            if (!isCreated.Succeeded)
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = isCreated.Errors.Select(e => e.Description).ToList()
                });

            var token = GenerateToken(newUser);
            return Ok(new AuthResult()
            {
                Result = true,
                Token = token
            });
        }

        [Route("Login")]
        [HttpPost]

        public async Task<IActionResult> Login([FromBody] UserLoginReqDto loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });

            var existingUser = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (existingUser == null)
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid Authentication Request"
                    }
                });

            var isCorrect = await _userManager.CheckPasswordAsync(existingUser, loginRequest.Password);
            if (!isCorrect)
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid Authentication Request"
                    }
                });

            var token = GenerateToken(existingUser);
            return Ok(new AuthResult()
            {
                Result = true,
                Token = token
            });

        }


        private string GenerateToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["AppSettings:Issuer"], // Add issuer
                Audience = _configuration["AppSettings:Audience"], // Add audience
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
