using Microsoft.EntityFrameworkCore;
using ShortenUrl.Models;

namespace ShortenUrl.Services
{
	class UrlService
	{
		private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		private const int CharacterCount = 62;
		private const int ShortUrlLength = 6;
		private static readonly Random random = new();
		private readonly ApiDbContext _context;

		public UrlService(ApiDbContext context)
		{
			_context = context;
		}
		private static string GetShorterUrl()
		{
			return new string(Enumerable.Range(0, ShortUrlLength).Select(_ => Characters[random.Next(CharacterCount)]).ToArray());
		}

		public string GenerateShortenUrl()
		{
			var shortUrl = GetShorterUrl();
			while (IsUrlNotUnique(shortUrl))
			{
				shortUrl = GetShorterUrl();
			}
			return shortUrl;
		}

		public bool IsUrlNotUnique(string input)
		{
			return _context.Urls.Any(u => u.ShortenUrl == input);
		}

		public Url SaveUrl(UrlDto urlDto, string baseUrl)
		{
			var user = _context.Users.Find(urlDto.UserId);

			var validSeconds = urlDto.ValidMinutes * 60;
			var token = GenerateShortenUrl();
			var url = new Url
			{
				OriginalUrl = urlDto.LongUrl,
				ShortenUrl = $"{baseUrl}/{token}",
				Token = token,
				ExpiryDate = DateTimeOffset.Now.AddSeconds(validSeconds),
				User = user!
			};
			_context.Urls.Add(url);
			_context.SaveChanges();
			return url;
		}

		public Url? GetUrl(string token)
		{
			var url = _context.Urls
				.Where(url => url.Token == token)
				.SingleOrDefault();
			var now = DateTime.Now;
			return url?.ExpiryDate > now ? url : null;
        }

		public List<Url> GetUrls(int userId)
		{
			return _context.Urls.Where(url => url.User.Id == userId).ToList();
		}
	}
}