using FluentValidation;
using ShortenUrl.Models.DTO;

namespace ShortenUrl.Models.Validators
{
    public class UserValidator : AbstractValidator<UserAuthDto>
    {
        public UserValidator()
        {
            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Your password must be at least 8 characters long.")
                .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
                .Matches(@"[\!\?\*\.]+").WithMessage("Your password must contain at least one (!? *.).");
        }
    }
}
