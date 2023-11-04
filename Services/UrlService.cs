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
			var validSeconds = urlDto.ValidMinutes * 60;
			var token = GenerateShortenUrl();
			var url = new Url
			{
				OriginalUrl = urlDto.LongUrl,
				ShortenUrl = $"{baseUrl}/{token}",
				Token = token,
				ExpiryDate = DateTimeOffset.Now.AddSeconds(validSeconds)
			};
			_context.Urls.Add(url);
			_context.SaveChanges();
			return url;
		}

		public Url? GetUrl(string token)
		{
			return _context.Urls.Where(u => u.Token == token).FirstOrDefault();
		}

		public List<Url> GetUrls()
		{
			return _context.Urls.ToList();
		}
	}
}