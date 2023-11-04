namespace ShortenUrl.Models
{
	public class UrlDto
	{
		public int ValidMinutes { get; set; }
		public string LongUrl { get; set; } = string.Empty;
	}
}