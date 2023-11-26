using System.ComponentModel.DataAnnotations.Schema;

namespace ShortenUrl.Models
{
	public class Url
	{
		public int Id { get; set; }
		public string OriginalUrl { get; set; } = string.Empty;
		public string ShortenUrl { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
		public DateTimeOffset ExpiryDate { get; set; } = DateTimeOffset.Now;
		public bool Archived { get; set; } = false;
		public DateTimeOffset DateCreated { get; set; }
		public DateTimeOffset? DateModified { get; set; }

        public required User User { get; set; }

		[NotMapped]
		public bool IsExpired => DateTimeOffset.Now > ExpiryDate;
	}
}