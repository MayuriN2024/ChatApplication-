using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PingMe.Server.Data;
using PingMe.Server.DTOs;
using PingMe.Server.Models;

namespace PingMe.Server.Services
{
    public interface IAuthService
    {
        Task<UserDTO> SignupAsync(RegisterRequestDTO registerRequest);
        Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequest);
        Task<Dictionary<string, object>> GetOnlineUsersAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly ChatDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ChatDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<UserDTO> SignupAsync(RegisterRequestDTO registerRequest)
        {
            var user = new User
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password) // Using BCrypt like standard practice
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTO { Username = user.Username, Email = user.Email };
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);

            return new LoginResponseDTO
            {
                Token = token,
                User = new UserDTO { Username = user.Username, Email = user.Email }
            };
        }

        public async Task<Dictionary<string, object>> GetOnlineUsersAsync()
        {
            var users = await _context.Users.Where(u => u.IsOnline).ToListAsync();
            var count = users.Count();

            return new Dictionary<string, object>
            {
                { "online_count", count },
                { "online_users", users.Select(u => new UserDTO { Username = u.Username, Email = u.Email }) }
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "VERY_LONG_AND_SECURE_SECRET_FOR_JWT_TOKEN_1234567890");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"] ?? "60")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
