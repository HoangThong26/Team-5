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

    public class EvaluationRequestValidator : AbstractValidator<EvaluationRequest>
    {
        public EvaluationRequestValidator()
        {
            RuleFor(x => x.ReportId)
                .NotEmpty().WithMessage("Report ID is required.")
                .GreaterThan(0).WithMessage("Invalid Report ID.");

            RuleFor(x => x.Score)
                .InclusiveBetween(0, 10).WithMessage("Score must be between 0 and 10.")
                .NotNull().WithMessage("Score cannot be empty.");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }

    public class AdminSetupRequestValidator : AbstractValidator<AdminSetupRequest>
    {
        public AdminSetupRequestValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Project start date is required.")
                .Must(BeAValidStartDate).WithMessage("Start date cannot be in the past.")
                .LessThan(DateTime.Now.AddYears(1)).WithMessage("Start date is too far in the future.");
        }

        private bool BeAValidStartDate(DateTime startDate)
        {
            return startDate.Date >= DateTime.Today;
        }
    }
}