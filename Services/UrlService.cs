using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using ShortenUrl.Models;
using ShortenUrl.Models.DTO;
using ShortenUrl.Models.Validators;
using System.Collections.Immutable;

namespace ShortenUrl.Services
{
	class UrlService
	{
		private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		private const int CharacterCount = 62;
		private const int ShortUrlLength = 6;
		private static readonly Random random = new();
		private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;

        public UrlService(ApiDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
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
            decimal hours = urlDto.ValidMinutes / 60;
            hours = decimal.Round(hours, 2);
            if (hours > 1)
            {
                throw new Exception("Can't create a URL valid for more than an hour.");
            }

            var urlValidator = new UrlValidator();
			var validationResult = urlValidator.Validate(urlDto);

			if (!validationResult.IsValid)
			{
				var errorMessage = validationResult.Errors.Select(e => e.ErrorMessage);
                throw new InvalidDataException(string.Join(", ", errorMessage));
			}

			var user = _context.Users.SingleOrDefault(u => u.UserId == urlDto.UserId);
			var validSeconds = urlDto.ValidMinutes * 60;
			var token = GenerateShortenUrl();
			var frontEndDomain = _configuration.GetSection("FrontEndDomain").Value;
            var url = new Url
			{
				OriginalUrl = urlDto.LongUrl,
				ShortenUrl = $"{frontEndDomain}/{token}",
				Token = token,
				ExpiryDate = DateTimeOffset.Now.AddSeconds(validSeconds),
				User = user!,
				DateCreated = DateTimeOffset.Now,
			};
			_context.Urls.Add(url);
			_context.SaveChanges();
			return url;
		}

		public Url? GetUrl(string token)
		{
			var url = _context.Urls
				.Where(url => url.Token == token && !url.Archived)
				.SingleOrDefault();
			var now = DateTime.Now;
			return url?.ExpiryDate > now ? url : null;
		}

		public List<UrlResponseDto> GetUrls(string userId)
		{
			return _context.Urls
				.Where(url => url.User.UserId == Guid.Parse(userId) && !url.Archived)
				.ToList()
				.Select(u => new UrlResponseDto 
				{
					Id = u.Id,
					ExpiryDate = u.ExpiryDate,
					OriginalUrl = u.OriginalUrl,
					ShortenUrl = u.ShortenUrl,
					Token = u.Token,
					IsExpired = u.IsExpired
				})
				.ToList();
		}

		public void ArchiveUrl(string urlToken)
		{
			var url = _context.Urls.Single(u => u.Token == urlToken);
			if (url is null)
			{
				throw new Exception("Url not found");
			}

			url.Archived = true;
			_context.SaveChanges();
		}
	}
}