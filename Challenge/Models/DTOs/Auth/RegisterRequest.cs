namespace Challenge.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}