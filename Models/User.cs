namespace ShortenUrl.Models
{
    public class User
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Url>? Urls { get; set; }
    }
}
