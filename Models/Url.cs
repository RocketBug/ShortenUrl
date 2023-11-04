namespace ShortenUrl.Models
{
	public class Url
	{
		public int Id { get; set; }
		public string OriginalUrl { get; set; } = string.Empty;
		public string ShortenUrl { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
		public DateTimeOffset ExpiryDate { get; set; } = DateTimeOffset.Now;
	}
}