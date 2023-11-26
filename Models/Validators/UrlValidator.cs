using FluentValidation;
using ShortenUrl.Models.DTO;

namespace ShortenUrl.Models.Validators
{
    public class UrlValidator : AbstractValidator<UrlDto>
    {
        public UrlValidator()
        {
            RuleFor(url => url.LongUrl)
                .Must(ValidateUrl)
                .WithMessage("Invalid URL");
        }

        private bool ValidateUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) && 
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
