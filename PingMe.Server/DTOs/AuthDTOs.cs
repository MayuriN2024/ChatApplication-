namespace PingMe.Server.DTOs
{
    public class RegisterRequestDTO
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginRequestDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class UserDTO
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
    }

    public class LoginResponseDTO
    {
        public required string Token { get; set; }
        public required UserDTO User { get; set; }
    }
}
