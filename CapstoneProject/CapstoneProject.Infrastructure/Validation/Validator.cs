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

        public class AdminCreateUserRequestValidator : AbstractValidator<AdminCreateUserRequest>
        {
            public AdminCreateUserRequestValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("Invalid email format.")
                    .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

                RuleFor(x => x.FullName)
                    .NotEmpty().WithMessage("Full name is required.")
                    .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
                    .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

                RuleFor(x => x.Phone)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$").WithMessage("Invalid phone number. It must start with 0 and contain exactly 10 digits.");
                RuleFor(x => x.Role)
                    .NotEmpty().WithMessage("Role is required.");
                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                    .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                    .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                    .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                    .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
            }
        }

        public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
        {
            public UpdateProfileRequestValidator()
            {
                RuleFor(x => x.FullName)
                    .NotEmpty().WithMessage("Full name is required.")
                    .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
                    .MaximumLength(255).WithMessage("Full name must not exceed 255 characters.");
                RuleFor(x => x.Phone)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$")
                    .WithMessage("Invalid phone number. It must start with 0 and contain exactly 10 digits.");
            }
        }
    }
}