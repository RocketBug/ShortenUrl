using System.ComponentModel.DataAnnotations.Schema;

namespace ShortenUrl.Models
{
    public class User
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool Archived { get; set; } = false;

        public string RefreshToken { get; set; } = string.Empty;
        public bool IsRefreshTokenRevoked { get; set; }
        public DateTimeOffset DateRefreshTokenCreated { get; set; }
        public DateTimeOffset DateRefreshTokenExpires { get; set; }
        public DateTimeOffset? DateCreated { get; set; }
        public DateTimeOffset? DateModified { get; set; }

        public ICollection<Url>? Urls { get; set; }


        [NotMapped]
        public string Token { get; set; } = string.Empty;
    }
}
