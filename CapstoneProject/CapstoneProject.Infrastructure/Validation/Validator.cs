using CapstoneProject.Application.DTO;
using FluentValidation;
using static System.Net.Mime.MediaTypeNames;
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
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }

    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters.");

            RuleFor(x => x.Phone)
                .Matches(@"^(03|05|07|08|09)\d{8}$")
                .WithMessage("Invalid Vietnamese phone number format.")
                .When(x => !string.IsNullOrEmpty(x.Phone));
        }
    }
}