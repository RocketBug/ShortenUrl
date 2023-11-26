namespace ShortenUrl.Models.DTO
{
    public class UserAuthDto
    {
        public required string UserName { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }
}
