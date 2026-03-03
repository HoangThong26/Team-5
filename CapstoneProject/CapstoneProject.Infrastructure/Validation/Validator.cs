using CapstoneProject.Application.DTO;
using FluentValidation;


namespace CapstoneProject.Infrastructure.Validation
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        }
    }
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(2).WithMessage("Password must be at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.");
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MinimumLength(8).WithMessage("Full name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^(03|05|07|08|09|01[2|6|8|9])([0-9]{8})$")
                .WithMessage("Invalid Vietnamese phone number format.");
        }
    }

    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters.");

            RuleFor(x => x.PasswordHash)
                .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .When(x => !string.IsNullOrEmpty(x.PasswordHash));

            RuleFor(x => x.Phone)
                .Matches(@"^(03|05|07|08|09|01[2|6|8|9])([0-9]{8})$")
                .WithMessage("Invalid Vietnamese phone number format.")
                .When(x => !string.IsNullOrEmpty(x.Phone));
            RuleFor(x => x.AvatarUrl)
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Invalid Avatar URL format.");
        }
    }
}
