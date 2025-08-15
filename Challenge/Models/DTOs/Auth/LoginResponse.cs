
namespace Challenge.Models.DTOs.Auth
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}