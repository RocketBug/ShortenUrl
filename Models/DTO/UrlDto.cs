namespace ShortenUrl.Models.DTO
{
    public class UrlDto
    {
        public Guid UserId { get; set; }
        public int ValidMinutes { get; set; }
        public string LongUrl { get; set; } = string.Empty;
    }
}