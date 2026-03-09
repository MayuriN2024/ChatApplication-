using Microsoft.AspNetCore.Authorization;
using PingMe.Server.DTOs;
using PingMe.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PingMe.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace PingMe.Server.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ChatDbContext _context;

        public AuthController(IAuthService authService, ChatDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<UserDTO>> Signup([FromBody] RegisterRequestDTO registerRequest)
        {
            var user = await _authService.SignupAsync(registerRequest);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login([FromBody] LoginRequestDTO loginRequest)
        {
            var loginResponse = await _authService.LoginAsync(loginRequest);
            if (loginResponse == null)
                return Unauthorized();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(60),
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("JWT", loginResponse.Token, cookieOptions);

            return Ok(loginResponse.User);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWT");
            return Ok("Logged out successfully");
        }

        [HttpGet("getonlineusers")]
        public async Task<ActionResult<Dictionary<string, object>>> GetOnlineUsers()
        {
            var result = await _authService.GetOnlineUsersAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("getcurrentuser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (username == null)
                return Unauthorized("USER NOT AUTHORIZED");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return NotFound("User not found");

            return Ok(new UserDTO { Username = user.Username, Email = user.Email });
        }
    }
}
